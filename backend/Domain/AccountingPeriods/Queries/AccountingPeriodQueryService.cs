using Domain.FundGoals;
using Domain.Transactions.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Service for querying Accounting Periods and their balances.
/// </summary>
public sealed class AccountingPeriodQueryService(
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService,
    TransactionQueryService transactionQueryService,
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
    /// Retrieves an Accounting Period with its Transactions and interpreted totals.
    /// </summary>
    public async Task<AccountingPeriodTransactions?> GetWithTransactionsAsync(
        AccountingPeriodTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodBalance? balance = await GetBalanceByIdAsync(query.AccountingPeriodId, cancellationToken);
        if (balance == null)
        {
            return null;
        }

        QueryPage<TransactionDetails> transactions = await transactionQueryService.GetAsync(
            new TransactionQuery(
                new TransactionFilter([query.AccountingPeriodId], [], [], [], []),
                query.Sort,
                query.Offset,
                query.Limit),
            cancellationToken);
        IReadOnlyCollection<Guid> ids = [query.AccountingPeriodId];
        IReadOnlyCollection<FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(ids, cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(ids, cancellationToken);
        var totals = FinancialRangeTotals.Calculate(incomeFacts, spendingFacts);
        return new AccountingPeriodTransactions(
            balance,
            transactions,
            totals.TotalIncome,
            totals.TrackedIncome,
            totals.TotalSpending);
    }

    /// <summary>
    /// Retrieves and interprets the requested Accounting Period range.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult> GetRangeAsync(
        AccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult(null, resolution.Failure);
        }

        AccountingPeriod start = resolution.AccountingPeriods.First();
        AccountingPeriod end = resolution.AccountingPeriods.Last();
        IReadOnlyCollection<AccountingPeriodBalance> periods = await accountingPeriodQueryRepository.GetRangeBalancesAsync(
            AccountingPeriodRangeResolver.GetChronologicalIndex(start),
            AccountingPeriodRangeResolver.GetChronologicalIndex(end),
            cancellationToken);
        if (!AccountingPeriodRangeResolver.IsContiguous(periods.Select(period => period.AccountingPeriod), start, end))
        {
            return new AccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.NotContiguous);
        }
        periods = await EnrichBalancesAsync(periods, cancellationToken);

        IReadOnlyCollection<Guid> ids = periods.Select(period => period.AccountingPeriod.Id.Value).ToList();
        IReadOnlyCollection<FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(ids, cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(ids, cancellationToken);
        var totals = FinancialRangeTotals.Calculate(incomeFacts, spendingFacts);
        IReadOnlyCollection<AccountingPeriodBalance> items = Sort(periods, query.Sort)
            .Skip(query.Offset)
            .Take(query.Limit ?? int.MaxValue)
            .ToList();
        var range = new AccountingPeriodRange(
            new QueryPage<AccountingPeriodBalance>(items, periods.Count),
            totals.TotalIncome,
            totals.TrackedIncome,
            totals.TotalSpending);
        return new AccountingPeriodRangeQueryResult(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Adds calculated income and Fund Goal requirement totals to period balances.
    /// </summary>
    private async Task<IReadOnlyCollection<AccountingPeriodBalance>> EnrichBalancesAsync(
        IReadOnlyCollection<AccountingPeriodBalance> balances,
        CancellationToken cancellationToken)
    {
        // Income facts are not currently partitioned by period. Query them one period at a time
        // to preserve the existing facts contract.
        var result = new List<AccountingPeriodBalance>();
        foreach (AccountingPeriodBalance balance in balances)
        {
            IReadOnlyCollection<FinancialRangeIncomeFact> periodIncomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(
                [balance.AccountingPeriod.Id.Value], cancellationToken);
            var incomeTotals = FinancialRangeTotals.Calculate(periodIncomeFacts, []);
            AccountingPeriodBalanceHistory history = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(balance.AccountingPeriod.Id);
            IReadOnlyCollection<FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriod(balance.AccountingPeriod.Id);
            decimal expectedGoalContributions = fundGoals
                .Sum(goal => FundGoalProgressService.CalculateRecommendedContribution(
                    history.FundBalances.SingleOrDefault(item => item.Fund.Id == goal.Fund.Id)?.OpeningBalance ?? 0,
                    goal.RegularContribution,
                    goal.MinimumFundedBalance,
                    goal.MaximumFundedBalance));
            decimal actualGoalContributions = fundGoals
                .Sum(goal => history.FundGoalTotals
                    .SingleOrDefault(item => item.Fund.Id == goal.Fund.Id)
                    ?.GetTotals().RegularAmountAssigned ?? 0);
            result.Add(balance with
            {
                ActualIncome = incomeTotals.TotalIncome,
                ActualTrackedIncome = incomeTotals.TrackedIncome,
                ExpectedGoalContributions = expectedGoalContributions,
                ActualGoalContributions = actualGoalContributions,
            });
        }
        return result;
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
