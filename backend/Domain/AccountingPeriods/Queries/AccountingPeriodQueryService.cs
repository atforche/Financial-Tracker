using Domain.Accounts;
using Domain.Transactions.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Service for querying Accounting Periods and their balances.
/// </summary>
public sealed class AccountingPeriodQueryService(
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    TransactionQueryService transactionQueryService)
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
    public Task<QueryPage<AccountingPeriodBalance>> GetWithBalancesAsync(
        AccountingPeriodBalanceQuery query,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetBalancesAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID, or null when it does not exist.
    /// </summary>
    public Task<AccountingPeriodBalance?> GetByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetBalanceByIdAsync(new AccountingPeriodId(accountingPeriodId), cancellationToken);

    /// <summary>
    /// Retrieves an Accounting Period with its Transactions and interpreted totals.
    /// </summary>
    public async Task<AccountingPeriodTransactions?> GetWithTransactionsAsync(
        AccountingPeriodTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodBalance? balance = await accountingPeriodQueryRepository.GetBalanceByIdAsync(
            new AccountingPeriodId(query.AccountingPeriodId),
            cancellationToken);
        if (balance == null)
        {
            return null;
        }

        QueryPage<TransactionDetails> transactions = await transactionQueryService.GetAsync(
            new TransactionQuery(
                new TransactionFilter([query.AccountingPeriodId], [], []),
                query.Sort,
                query.Offset,
                query.Limit),
            cancellationToken);
        IReadOnlyCollection<Guid> ids = [query.AccountingPeriodId];
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(ids, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(ids, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> recognizedIncome = incomeFacts
            .Where(fact => !fact.HasInternalSource || fact.PostedDate != null)
            .ToList();
        decimal totalIncome = recognizedIncome.Sum(fact => fact.Amount);
        decimal trackedIncome = recognizedIncome.Where(fact => fact.AccountType.IsTracked()).Sum(fact => fact.Amount);
        decimal totalSpending = spendingFacts.Where(fact => fact.PostedDate != null).Sum(fact => fact.Amount);
        return new AccountingPeriodTransactions(
            balance,
            transactions,
            totalIncome,
            trackedIncome,
            totalSpending);
    }

    /// <summary>
    /// Retrieves and interprets the requested Accounting Period range.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult> GetRangeAsync(
        AccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<AccountingPeriod> endpoints = await accountingPeriodQueryRepository.GetEndpointsAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == query.StartId);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == query.EndId);
        AccountingPeriodRangeQueryFailure failure = AccountingPeriodRangeQueryFailure.None;
        if (start == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.StartNotFound;
        }
        if (end == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.EndNotFound;
        }
        if (failure != AccountingPeriodRangeQueryFailure.None)
        {
            return new AccountingPeriodRangeQueryResult(null, failure);
        }

        int startIndex = GetChronologicalIndex(start!);
        int endIndex = GetChronologicalIndex(end!);
        if (startIndex > endIndex)
        {
            return new AccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.Reversed);
        }

        IReadOnlyCollection<AccountingPeriodBalance> periods = await accountingPeriodQueryRepository.GetRangeBalancesAsync(
            startIndex,
            endIndex,
            cancellationToken);
        IReadOnlyCollection<int> persistedIndexes = periods
            .Select(period => GetChronologicalIndex(period.AccountingPeriod))
            .Order()
            .ToList();
        if (!persistedIndexes.SequenceEqual(Enumerable.Range(startIndex, endIndex - startIndex + 1)))
        {
            return new AccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.NotContiguous);
        }

        IReadOnlyCollection<Guid> ids = periods.Select(period => period.AccountingPeriod.Id.Value).ToList();
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(ids, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(ids, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> recognizedIncome = incomeFacts
            .Where(fact => !fact.HasInternalSource || fact.PostedDate != null)
            .ToList();
        decimal totalIncome = recognizedIncome.Sum(fact => fact.Amount);
        decimal trackedIncome = recognizedIncome.Where(fact => fact.AccountType.IsTracked()).Sum(fact => fact.Amount);
        decimal totalSpending = spendingFacts.Where(fact => fact.PostedDate != null).Sum(fact => fact.Amount);
        IReadOnlyCollection<AccountingPeriodBalance> items = Sort(periods, query.Sort)
            .Skip(query.Offset)
            .Take(query.Limit ?? int.MaxValue)
            .ToList();
        var range = new AccountingPeriodRange(
            new QueryPage<AccountingPeriodBalance>(items, periods.Count),
            totalIncome,
            trackedIncome,
            totalSpending);
        return new AccountingPeriodRangeQueryResult(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Calculates a chronological index for an Accounting Period based on its year and month.
    /// </summary>
    private static int GetChronologicalIndex(AccountingPeriod accountingPeriod) =>
        (accountingPeriod.Year * 12) + accountingPeriod.Month;

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