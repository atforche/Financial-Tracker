using Domain.AccountGoals;
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
    IFundGoalRepository fundGoalRepository,
    IAccountGoalRepository accountGoalRepository)
{
    /// <summary>
    /// Retrieves Accounting Periods matching the provided query.
    /// </summary>
    public Task<QueryPage<AccountingPeriod>> GetAsync(
        AccountingPeriodQuery query,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves the earliest and latest Accounting Periods, when any exist.
    /// </summary>
    public Task<(AccountingPeriod? First, AccountingPeriod? Latest)> GetBoundsAsync(CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetBoundsAsync(cancellationToken);

    /// <summary>
    /// Retrieves Accounting Periods and their balances.
    /// </summary>
    public async Task<QueryPage<AccountingPeriodBalance>> GetWithBalancesAsync(
        AccountingPeriodBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        QueryPage<StoredAccountingPeriodBalance> page = await accountingPeriodQueryRepository.GetBalancesAsync(query, cancellationToken);
        return new QueryPage<AccountingPeriodBalance>(
            await EnrichBalancesAsync(page.Items.Select(item => item.AccountingPeriod).ToList(), cancellationToken),
            page.TotalCount);
    }

    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID, or null when it does not exist.
    /// </summary>
    public async Task<AccountingPeriodBalance?> GetBalanceByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriod? period = await GetWithExpectedIncomeSourcesByIdAsync(accountingPeriodId, cancellationToken);
        return period == null ? null : (await EnrichBalancesAsync([period], cancellationToken)).Single();
    }

    /// <summary>
    /// Retrieves an Accounting Period by ID, or null when it does not exist.
    /// </summary>
    public Task<AccountingPeriod?> GetByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetByIdAsync(new AccountingPeriodId(accountingPeriodId), cancellationToken);

    /// <summary>
    /// Retrieves the provided Accounting Periods in date order with their Expected Income Sources and the income received
    /// from each. Accounting Periods that do not exist are left out.
    /// </summary>
    public async Task<IReadOnlyCollection<AccountingPeriodExpectedIncome>> GetExpectedIncomeAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodQueryRepository.GetWithExpectedIncomeSourcesByIdsAsync(
            accountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList(),
            cancellationToken);
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetIncomeFactsByAccountingPeriodAsync(
            periods.Select(period => period.Id.Value).ToList(),
            cancellationToken);
        return periods
            .OrderBy(period => period.Year)
            .ThenBy(period => period.Month)
            .Select(period => new AccountingPeriodExpectedIncome(period, ReceivedIncomeBySource.From(incomeFacts[period.Id.Value])))
            .ToList();
    }

    /// <summary>
    /// Retrieves an Expected Income Source with its Accounting Period and linked Income Transactions, or null when the
    /// source does not exist in that Accounting Period.
    /// </summary>
    public Task<ExpectedIncomeSourceDetail?> GetExpectedIncomeSourceAsync(
        Guid accountingPeriodId,
        Guid expectedIncomeSourceId,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetExpectedIncomeSourceDetailAsync(
            new AccountingPeriodId(accountingPeriodId),
            expectedIncomeSourceId,
            cancellationToken);

    /// <summary>
    /// Retrieves an Accounting Period with its totals, goal summary, neighbors, and opening and closing balances, or null
    /// when it does not exist.
    /// </summary>
    public async Task<AccountingPeriodDetail?> GetDetailAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriod? period = await GetWithExpectedIncomeSourcesByIdAsync(accountingPeriodId, cancellationToken);
        if (period == null)
        {
            return null;
        }
        IReadOnlyCollection<Guid> ids = [accountingPeriodId];
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetIncomeFactsByAccountingPeriodAsync(ids, cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(ids, cancellationToken);
        (AccountingPeriod? previous, AccountingPeriod? next) = await accountingPeriodQueryRepository.GetAdjacentAsync(period, cancellationToken);
        AccountingPeriodBalanceHistory history = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(period.Id);
        IReadOnlyCollection<FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriod(period.Id);
        IReadOnlyCollection<AccountGoal> accountGoals = accountGoalRepository.GetAllByAccountingPeriod(period.Id);
        IReadOnlyDictionary<FundGoalId, FundGoalProgress> fundProgress = FundGoalService.GetProgresses(fundGoals, history);
        IReadOnlyDictionary<AccountGoalId, AccountGoalProgress> accountProgress = AccountGoalService.GetProgresses(accountGoals, history);
        var fundProgressByFund = fundGoals.Where(goal => fundProgress.ContainsKey(goal.Id))
            .ToDictionary(goal => goal.Fund.Id, goal => fundProgress[goal.Id]);
        var accountProgressByAccount = accountGoals.Where(goal => accountProgress.ContainsKey(goal.Id))
            .ToDictionary(goal => goal.Account.Id, goal => accountProgress[goal.Id]);
        return new AccountingPeriodDetail(
            EnrichBalance(period, history, fundGoals, incomeFacts[accountingPeriodId], out int cappedGoalCount),
            cappedGoalCount,
            AccountingPeriodGoalSummary.From(
                fundProgress.Values.ToList(),
                accountProgress.Values.ToList()),
            previous,
            next,
            FinancialRangeTotals.Calculate([], spendingFacts).TotalSpending,
            AccountBalanceSummary.Create(history.AccountBalances.Select(item => (item.Account.Type, item.OpeningBalance))),
            AccountBalanceSummary.Create(history.AccountBalances.Select(item => (item.Account.Type, item.ClosingBalance))),
            FundBalanceSummary.Create(history.FundBalances.Select(item => (item.Fund.Id, item.OpeningBalance))),
            FundBalanceSummary.Create(history.FundBalances.Select(item => (item.Fund.Id, item.ClosingBalance))),
            history.AccountBalances.Select(item => new AccountingPeriodAccount(
                item.Account,
                item.OpeningBalance,
                item.ClosingBalance,
                accountProgressByAccount.GetValueOrDefault(item.Account.Id))).ToList(),
            history.FundBalances.Select(item => new AccountingPeriodFund(
                item.Fund,
                item.OpeningBalance,
                item.ClosingBalance,
                fundProgressByFund.GetValueOrDefault(item.Fund.Id))).ToList());
    }

    /// <summary>
    /// Retrieves the requested Accounting Period range with each period's balance and the range's total spending.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<AccountingPeriodRange>> GetRangeAsync(
        AccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        (AccountingPeriod? start, AccountingPeriod? end, AccountingPeriodRangeQueryFailure failure) =
            await accountingPeriodRangeService.ResolveEndpointsAsync(query.StartId, query.EndId, cancellationToken);
        if (start == null || end == null)
        {
            return new AccountingPeriodRangeQueryResult<AccountingPeriodRange>(null, failure);
        }

        IReadOnlyCollection<StoredAccountingPeriodBalance> periods = await accountingPeriodQueryRepository.GetRangeBalancesAsync(
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
        var range = new AccountingPeriodRange(
            EnrichBalances(periods.Select(period => period.AccountingPeriod).ToList(), incomeFacts),
            FinancialRangeTotals.Calculate([], spendingFacts).TotalSpending);
        return new AccountingPeriodRangeQueryResult<AccountingPeriodRange>(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Gets the provided Accounting Periods' balances with their calculated income and Fund Goal requirement totals.
    /// </summary>
    private async Task<IReadOnlyCollection<AccountingPeriodBalance>> EnrichBalancesAsync(
        IReadOnlyCollection<AccountingPeriod> periods,
        CancellationToken cancellationToken)
    {
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetIncomeFactsByAccountingPeriodAsync(
            periods.Select(period => period.Id.Value).ToList(),
            cancellationToken);
        return EnrichBalances(periods, incomeFacts);
    }

    /// <summary>
    /// Retrieves an Accounting Period with its Expected Income Sources by ID, or null when it does not exist.
    /// </summary>
    private async Task<AccountingPeriod?> GetWithExpectedIncomeSourcesByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken) =>
        (await accountingPeriodQueryRepository.GetWithExpectedIncomeSourcesByIdsAsync(
            [new AccountingPeriodId(accountingPeriodId)],
            cancellationToken)).SingleOrDefault();

    /// <summary>
    /// Gets the provided Accounting Periods' balances with their calculated income and Fund Goal requirement totals, using
    /// already-loaded income facts.
    /// </summary>
    private List<AccountingPeriodBalance> EnrichBalances(
        IReadOnlyCollection<AccountingPeriod> periods,
        ILookup<Guid, FinancialRangeIncomeFact> incomeFacts)
    {
        var ids = periods.Select(period => period.Id).ToList();
        var histories = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriods(ids)
            .ToDictionary(history => history.AccountingPeriod.Id);
        ILookup<AccountingPeriodId, FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriods(ids)
            .ToLookup(goal => goal.AccountingPeriod!.Id);
        return periods.Select(period => EnrichBalance(
            period,
            histories[period.Id],
            fundGoals[period.Id].ToList(),
            incomeFacts[period.Id.Value],
            out _)).ToList();
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
        return EnrichBalance(accountingPeriod, history, fundGoals, incomeFacts[accountingPeriod.Id.Value], out _);
    }

    /// <summary>
    /// Gets an Accounting Period's balance with its calculated income and Fund Goal requirement totals, and how many
    /// of its Fund Goals expect less than they plan because of their maximum ending balance.
    /// </summary>
    private static AccountingPeriodBalance EnrichBalance(
        AccountingPeriod accountingPeriod,
        AccountingPeriodBalanceHistory history,
        IReadOnlyCollection<FundGoal> fundGoals,
        IEnumerable<FinancialRangeIncomeFact> incomeFacts,
        out int cappedGoalCount)
    {
        var facts = incomeFacts.ToList();
        var incomeTotals = FinancialRangeTotals.Calculate(facts, []);
        (decimal expectedGoalContributions, decimal actualGoalContributions, decimal unplannedGoalContributions, cappedGoalCount) =
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
            unplannedGoalContributions,
            ReceivedIncomeBySource.From(facts));
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
}
