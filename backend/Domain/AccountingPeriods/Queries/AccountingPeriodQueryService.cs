using Domain.Accounts.Queries;
using Domain.FundGoals;
using Domain.Funds.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Service for querying Accounting Periods and their balances.
/// </summary>
public sealed class AccountingPeriodQueryService(
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IFundGoalRepository fundGoalRepository)
{
    /// <summary>
    /// Retrieves Accounting Periods matching the provided query.
    /// </summary>
    public Task<QueryPage<AccountingPeriod>> GetAsync(
        AccountingPeriodQuery query,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Accounting Periods and their balances.
    /// </summary>
    public async Task<QueryPage<AccountingPeriodBalance>> GetWithBalancesAsync(
        AccountingPeriodBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        QueryPage<AccountingPeriodBalance> page = await accountingPeriodQueryRepository.GetBalancesAsync(query, cancellationToken);
        return new QueryPage<AccountingPeriodBalance>(
            await EnrichBalancesAsync(page.Items, cancellationToken),
            page.TotalCount);
    }

    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID, or null when it does not exist.
    /// </summary>
    public async Task<AccountingPeriodBalance?> GetBalanceByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodBalance? balance = await accountingPeriodQueryRepository.GetBalanceByIdAsync(new AccountingPeriodId(accountingPeriodId), cancellationToken);
        return balance == null ? null : (await EnrichBalancesAsync([balance], cancellationToken)).Single();
    }

    /// <summary>
    /// Retrieves an Accounting Period by ID, or null when it does not exist.
    /// </summary>
    public Task<AccountingPeriod?> GetByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetByIdAsync(new AccountingPeriodId(accountingPeriodId), cancellationToken);

    /// <summary>
    /// Retrieves an Accounting Period with its totals, neighbors, and opening and closing balances, or null when it
    /// does not exist.
    /// </summary>
    public async Task<AccountingPeriodDetail?> GetDetailAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodBalance? balance = await accountingPeriodQueryRepository.GetBalanceByIdAsync(
            new AccountingPeriodId(accountingPeriodId),
            cancellationToken);
        if (balance == null)
        {
            return null;
        }
        AccountingPeriod period = balance.AccountingPeriod;
        IReadOnlyCollection<Guid> ids = [accountingPeriodId];
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetIncomeFactsByAccountingPeriodAsync(ids, cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(ids, cancellationToken);
        (AccountingPeriod? previous, AccountingPeriod? next) = await accountingPeriodQueryRepository.GetAdjacentAsync(period, cancellationToken);
        AccountingPeriodBalanceHistory history = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(period.Id);
        IReadOnlyCollection<FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriod(period.Id);
        return new AccountingPeriodDetail(
            EnrichBalance(period, history, fundGoals, incomeFacts[accountingPeriodId]),
            GetGoalContributions(fundGoals, history).CappedCount,
            previous,
            next,
            FinancialRangeTotals.Calculate([], spendingFacts).TotalSpending,
            AccountBalanceSummary.Create(history.AccountBalances.Select(item => (item.Account.Type, item.OpeningBalance))),
            AccountBalanceSummary.Create(history.AccountBalances.Select(item => (item.Account.Type, item.ClosingBalance))),
            FundBalanceSummary.Create(history.FundBalances.Select(item => (item.Fund.Id, item.OpeningBalance))),
            FundBalanceSummary.Create(history.FundBalances.Select(item => (item.Fund.Id, item.ClosingBalance))));
    }

    /// <summary>
    /// Retrieves and interprets the requested Accounting Period range.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<AccountingPeriodRange>> GetRangeAsync(
        AccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult<AccountingPeriodRange>(null, resolution.Failure);
        }

        AccountingPeriod start = resolution.AccountingPeriods.First();
        AccountingPeriod end = resolution.AccountingPeriods.Last();
        IReadOnlyCollection<AccountingPeriodBalance> periods = await accountingPeriodQueryRepository.GetRangeBalancesAsync(
            AccountingPeriodRangeResolver.GetChronologicalIndex(start),
            AccountingPeriodRangeResolver.GetChronologicalIndex(end),
            cancellationToken);
        if (!AccountingPeriodRangeResolver.IsContiguous(periods.Select(period => period.AccountingPeriod), start, end))
        {
            return new AccountingPeriodRangeQueryResult<AccountingPeriodRange>(null, AccountingPeriodRangeQueryFailure.NotContiguous);
        }

        IReadOnlyCollection<Guid> ids = periods.Select(period => period.AccountingPeriod.Id.Value).ToList();
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetIncomeFactsByAccountingPeriodAsync(ids, cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(ids, cancellationToken);
        var totals = FinancialRangeTotals.Calculate(incomeFacts.SelectMany(group => group), spendingFacts);
        decimal totalExpectedIncome = periods.Sum(period =>
            period.AccountingPeriod.ExpectedIncomeSources.Sum(source => source.ExpectedAmount));
        decimal trackedExpectedIncome = periods.Sum(period =>
            period.AccountingPeriod.ExpectedIncomeSources.Sum(source => source.ExpectedTrackedAmount));
        // Sorting only uses persisted fields, so only the requested page needs its calculated totals
        IReadOnlyCollection<AccountingPeriodBalance> items = EnrichBalances(
            Sort(periods, query.Sort).Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToList(),
            incomeFacts);
        var range = new AccountingPeriodRange(
            new QueryPage<AccountingPeriodBalance>(items, periods.Count),
            totals.TotalIncome,
            totals.TrackedIncome,
            totals.TotalSpending,
            totalExpectedIncome,
            trackedExpectedIncome);
        return new AccountingPeriodRangeQueryResult<AccountingPeriodRange>(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Adds calculated income and Fund Goal requirement totals to period balances.
    /// </summary>
    private async Task<IReadOnlyCollection<AccountingPeriodBalance>> EnrichBalancesAsync(
        IReadOnlyCollection<AccountingPeriodBalance> balances,
        CancellationToken cancellationToken)
    {
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetIncomeFactsByAccountingPeriodAsync(
            balances.Select(balance => balance.AccountingPeriod.Id.Value).ToList(),
            cancellationToken);
        return EnrichBalances(balances, incomeFacts);
    }

    /// <summary>
    /// Adds calculated income and Fund Goal requirement totals to period balances using already-loaded income facts.
    /// </summary>
    private List<AccountingPeriodBalance> EnrichBalances(
        IReadOnlyCollection<AccountingPeriodBalance> balances,
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts)
    {
        var ids = balances.Select(balance => balance.AccountingPeriod.Id).ToList();
        var histories = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriods(ids)
            .ToDictionary(history => history.AccountingPeriod.Id);
        ILookup<AccountingPeriodId, FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriods(ids)
            .ToLookup(goal => goal.AccountingPeriod!.Id);
        return balances.Select(balance => EnrichBalance(
            balance.AccountingPeriod,
            histories[balance.AccountingPeriod.Id],
            fundGoals[balance.AccountingPeriod.Id].ToList(),
            incomeFacts[balance.AccountingPeriod.Id.Value])).ToList();
    }

    /// <summary>
    /// Gets an Accounting Period's balance with its calculated income and Fund Goal requirement totals, from its
    /// already-loaded balance history and Fund Goals.
    /// </summary>
    internal async Task<AccountingPeriodBalance> EnrichBalanceAsync(
        AccountingPeriod accountingPeriod,
        AccountingPeriodBalanceHistory history,
        IReadOnlyCollection<FundGoal> fundGoals,
        CancellationToken cancellationToken)
    {
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetIncomeFactsByAccountingPeriodAsync(
            [accountingPeriod.Id.Value], cancellationToken);
        return EnrichBalance(accountingPeriod, history, fundGoals, incomeFacts[accountingPeriod.Id.Value]);
    }

    /// <summary>
    /// Gets an Accounting Period's balance with its calculated income and Fund Goal requirement totals.
    /// </summary>
    private static AccountingPeriodBalance EnrichBalance(
        AccountingPeriod accountingPeriod,
        AccountingPeriodBalanceHistory history,
        IReadOnlyCollection<FundGoal> fundGoals,
        IEnumerable<FinancialRangeIncomeFact> incomeFacts)
    {
        var incomeTotals = FinancialRangeTotals.Calculate(incomeFacts, []);
        (decimal expectedGoalContributions, decimal actualGoalContributions, decimal unplannedGoalContributions, _) =
            GetGoalContributions(fundGoals, history);
        return new AccountingPeriodBalance(
            accountingPeriod,
            history.OpeningBalance,
            history.ClosingBalance,
            incomeTotals.TotalIncome,
            incomeTotals.TrackedIncome,
            incomeTotals.UnplannedIncome,
            incomeTotals.UnplannedTrackedIncome,
            fundGoals.Sum(goal => goal.PlannedMonthlyContribution ?? 0),
            expectedGoalContributions,
            actualGoalContributions,
            unplannedGoalContributions);
    }

    /// <summary>
    /// Gets the contributions expected toward the provided Fund Goals, the amount assigned to them so far that counts
    /// toward those contributions, the amount assigned that does not, and how many of them expect less than they plan
    /// because of their maximum ending balance.
    /// </summary>
    private static (decimal Expected, decimal Actual, decimal Unplanned, int CappedCount) GetGoalContributions(
        IReadOnlyCollection<FundGoal> fundGoals,
        AccountingPeriodBalanceHistory history)
    {
        decimal expected = 0;
        decimal actual = 0;
        decimal unplanned = 0;
        int cappedCount = 0;
        foreach (FundGoal goal in fundGoals)
        {
            FundGoalTotals? totals = history.FundGoalTotals.SingleOrDefault(item => item.Fund.Id == goal.Fund.Id)?.GetTotals();
            decimal assigned = totals?.AmountAssignedToExpectedContribution ?? 0;
            // Assignments that do not count toward the contribution, which come from unplanned income
            unplanned += (totals?.AmountAssigned ?? 0) - assigned;
            decimal goalExpected = FundGoalProgressService.CalculateExpectedContribution(
                history.FundBalances.SingleOrDefault(item => item.Fund.Id == goal.Fund.Id)?.ClosingBalance ?? 0,
                assigned,
                goal.PlannedMonthlyContribution,
                goal.MaximumEndingBalance,
                goal.AllowExpectedContributionAboveMaximum);
            if (goalExpected < Math.Max(goal.PlannedMonthlyContribution ?? 0, 0))
            {
                cappedCount++;
            }
            expected += goalExpected;
            actual += assigned;
        }
        return (expected, actual, unplanned, cappedCount);
    }

    /// <summary>
    /// Sorts the provided Accounting Period balances based on the specified sort criteria.
    /// </summary>
    private static IOrderedEnumerable<AccountingPeriodBalance> Sort(
        IEnumerable<AccountingPeriodBalance> periods,
        AccountingPeriodBalanceSort sort) => sort switch
        {
            AccountingPeriodBalanceSort.Date => periods.OrderBy(period => period.AccountingPeriod.Year).ThenBy(period => period.AccountingPeriod.Month).ThenBy(period => period.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.DateDescending => periods.OrderByDescending(period => period.AccountingPeriod.Year).ThenByDescending(period => period.AccountingPeriod.Month).ThenBy(period => period.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.IsOpen => periods.OrderBy(period => period.AccountingPeriod.IsOpen).ThenBy(period => period.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.IsOpenDescending => periods.OrderByDescending(period => period.AccountingPeriod.IsOpen).ThenBy(period => period.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.OpeningBalance => periods.OrderBy(period => period.OpeningBalance).ThenBy(period => period.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.OpeningBalanceDescending => periods.OrderByDescending(period => period.OpeningBalance).ThenBy(period => period.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.ClosingBalance => periods.OrderBy(period => period.ClosingBalance).ThenBy(period => period.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.ClosingBalanceDescending => periods.OrderByDescending(period => period.ClosingBalance).ThenBy(period => period.AccountingPeriod.Id),
            _ => periods.OrderByDescending(period => period.AccountingPeriod.Year).ThenByDescending(period => period.AccountingPeriod.Month).ThenBy(period => period.AccountingPeriod.Id),
        };
}
