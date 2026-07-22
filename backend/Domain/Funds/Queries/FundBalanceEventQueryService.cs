using Domain.BalanceEvents;
using Domain.Transactions;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Domain.Funds.Queries;

/// <summary>
/// Service for querying interpreted Fund balance events.
/// </summary>
public sealed class FundBalanceEventQueryService(IFundBalanceEventQueryRepository repository)
{
    /// <summary>
    /// Retrieves Fund balance events matching the provided query.
    /// </summary>
    public async Task<QueryPage<FundBalanceEvent>> GetAsync(
        FundBalanceEventQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await repository.GetTransactionsAsync(query.Start, query.End, cancellationToken);
        IReadOnlyCollection<AccountingPeriods.AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriods.AccountingPeriod> periods = await repository.GetAccountingPeriodsAsync(periodIds, cancellationToken);
        return await GetAsync(transactions, periods, query.Filter, query.Sort, query.Offset, query.Limit, cancellationToken);
    }

    /// <summary>
    /// Retrieves Fund balance events in the requested Accounting Period range.
    /// </summary>
    public async Task<FundBalanceEventAccountingPeriodRangeQueryResult> GetAsync(
        FundBalanceEventAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<AccountingPeriods.AccountingPeriodId> endpointIds =
            [new(query.StartId), new(query.EndId)];
        IReadOnlyCollection<AccountingPeriods.AccountingPeriod> endpoints = await repository.GetAccountingPeriodsAsync(
            endpointIds,
            cancellationToken);
        AccountingPeriods.AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == query.StartId);
        AccountingPeriods.AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == query.EndId);
        if (start == null || end == null)
        {
            return new FundBalanceEventAccountingPeriodRangeQueryResult(null);
        }

        int startIndex = GetChronologicalIndex(start);
        int endIndex = GetChronologicalIndex(end);
        if (startIndex > endIndex)
        {
            return new FundBalanceEventAccountingPeriodRangeQueryResult(null);
        }

        IReadOnlyCollection<AccountingPeriods.AccountingPeriod> periods = await repository.GetAccountingPeriodsAsync(
            startIndex,
            endIndex,
            cancellationToken);
        IReadOnlyCollection<int> persistedIndexes = periods.Select(GetChronologicalIndex).ToList();
        if (!persistedIndexes.SequenceEqual(Enumerable.Range(startIndex, endIndex - startIndex + 1)))
        {
            return new FundBalanceEventAccountingPeriodRangeQueryResult(null);
        }

        IReadOnlyCollection<AccountingPeriods.AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        IReadOnlyCollection<Transaction> transactions = await repository.GetTransactionsAsync(periodIds, cancellationToken);
        QueryPage<FundBalanceEvent> page = await GetAsync(
            transactions,
            periods,
            query.Filter,
            query.Sort,
            query.Offset,
            query.Limit,
            cancellationToken);
        return new FundBalanceEventAccountingPeriodRangeQueryResult(page);
    }

    /// <summary>
    /// Interprets and pages Fund balance events from the provided facts.
    /// </summary>
    private async Task<QueryPage<FundBalanceEvent>> GetAsync(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<AccountingPeriods.AccountingPeriod> accountingPeriods,
        FundFilter filter,
        FundBalanceEventSort sort,
        int offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        var periods = accountingPeriods.ToDictionary(period => period.Id);
        IReadOnlyCollection<FundId> fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null)).Distinct().ToList();
        var funds = (await repository.GetFundsAsync(fundIds, cancellationToken)).ToDictionary(fund => fund.Id);
        IReadOnlyCollection<FundBalanceHistory> histories = await repository.GetFundHistoriesAsync(fundIds, cancellationToken);
        IEnumerable<FundBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, histories))
            .Where(balanceEvent => Matches(balanceEvent.Fund, filter));
        var allItems = Sort(events, sort).ToList();
        return new QueryPage<FundBalanceEvent>(
            allItems.Skip(offset).Take(limit ?? int.MaxValue).ToList(),
            allItems.Count);
    }

    /// <summary>
    /// Calculates the chronological index of an Accounting Period.
    /// </summary>
    private static int GetChronologicalIndex(AccountingPeriods.AccountingPeriod period) =>
        (period.Year * 12) + period.Month;

    /// <summary>
    /// Retrieves interpreted Fund balance events for a Transaction.
    /// </summary>
    private static IEnumerable<FundBalanceEvent> GetEvents(
        Transaction transaction,
        AccountingPeriods.AccountingPeriod period,
        Dictionary<FundId, Fund> funds,
        IReadOnlyCollection<FundBalanceHistory> histories) => transaction switch
        {
            SpendingTransaction spending => spending.Destinations.SelectMany(destination => destination.FundAssignments)
                .Select(amount => Create(transaction, period, funds[amount.FundId], amount.Amount, BalanceEventType.Debit, histories)),
            IncomeTransaction income => income.Destinations.SelectMany(destination => destination.FundAssignments)
                .Select(amount => Create(transaction, period, funds[amount.FundId], amount.Amount, BalanceEventType.Credit, histories)),
            FundTransaction fund => new[] { Create(transaction, period, fund.Source.Fund, transaction.Amount, BalanceEventType.Debit, histories) }
                .Concat(fund.Destinations.Select(destination => Create(transaction, period, destination.Fund, destination.Amount, BalanceEventType.Credit, histories))),
            _ => [],
        };

    /// <summary>
    /// Creates an interpreted Fund balance event.
    /// </summary>
    private static FundBalanceEvent Create(
        Transaction transaction,
        AccountingPeriods.AccountingPeriod period,
        Fund fund,
        decimal amount,
        BalanceEventType type,
        IReadOnlyCollection<FundBalanceHistory> allHistories)
    {
        var histories = allHistories.Where(history => history.Fund.Id == fund.Id).ToList();
        FundBalanceHistory? current = histories.LastOrDefault(history => history.TransactionId == transaction.Id);
        int index = current == null ? -1 : histories.IndexOf(current);
        FundBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new FundBalanceEvent(
            period,
            transaction.Id,
            transaction.Date,
            type,
            amount,
            fund,
            ToBalance(fund, previous, fund.OnboardedBalance ?? 0),
            ToBalance(fund, current));
    }

    /// <summary>
    /// Creates a Fund balance from a Fund and its history.
    /// </summary>
    private static FundBalance ToBalance(Fund fund, FundBalanceHistory? history, decimal fallback = 0) => new(
        fund,
        history?.PostedBalance ?? fallback,
        history?.PendingDebitAmount ?? 0,
        history?.PendingCreditAmount ?? 0);

    /// <summary>
    /// Determines whether a Fund matches the provided filter.
    /// </summary>
    private static bool Matches(Fund fund, FundFilter filter) =>
        (string.IsNullOrWhiteSpace(filter.NameSearch) || fund.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase))
        && (filter.Names.Count == 0 || filter.Names.Contains(fund.Name));

    /// <summary>
    /// Sorts Fund balance events by the provided sort order.
    /// </summary>
    private static IOrderedEnumerable<FundBalanceEvent> Sort(
        IEnumerable<FundBalanceEvent> events,
        FundBalanceEventSort sort) => sort switch
        {
            FundBalanceEventSort.FundName => events.OrderBy(item => item.Fund.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.FundNameDescending => events.OrderByDescending(item => item.Fund.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.AccountingPeriod => events.OrderBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.AccountingPeriodDescending => events.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.Date => events.OrderBy(item => item.Date).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.DateDescending => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            _ => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        };
}