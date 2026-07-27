using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Transactions;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Spending;

namespace Domain.Funds.Queries;

/// <summary>
/// Service for querying interpreted Fund balance events.
/// </summary>
public sealed class FundBalanceEventQueryService(
    IFundBalanceEventQueryRepository repository,
    ITransactionBalanceEventQueryRepository transactionQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves fully projected Fund balance events for the requested Transactions.
    /// </summary>
    public async Task<IReadOnlyCollection<FundBalanceEvent>> GetForTransactionsAsync(
        IReadOnlyCollection<Transaction> requestedTransactions,
        CancellationToken cancellationToken = default)
    {
        if (requestedTransactions.Count == 0)
        {
            return [];
        }
        IReadOnlyCollection<FundId> requestedFundIds = requestedTransactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null)).Distinct().ToList();
        IReadOnlyCollection<Transaction> pendingTransactions = await transactionQueryRepository.GetPendingForFundsAsync(requestedFundIds, cancellationToken);
        IReadOnlyCollection<Transaction> transactions = requestedTransactions.Concat(pendingTransactions).DistinctBy(transaction => transaction.Id).ToList();
        IReadOnlyCollection<FundId> fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null)).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        var periods = (await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken)).ToDictionary(period => period.Id);
        var funds = (await repository.GetFundsAsync(fundIds, cancellationToken)).ToDictionary(fund => fund.Id);
        IReadOnlyCollection<FundBalanceHistory> histories = await repository.GetFundHistoriesAsync(fundIds, cancellationToken);
        var historiesByFund = histories.GroupBy(history => history.Fund.Id).ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, historiesByFund))
            .Where(balanceEvent => requestedFundIds.Contains(balanceEvent.Fund.Id)).ToList();
        events = ProjectPendingEvents(events, transactions, historiesByFund);
        var requestedIds = requestedTransactions.Select(transaction => transaction.Id).ToHashSet();
        return events.Where(balanceEvent => requestedIds.Contains(balanceEvent.TransactionId)).ToList();
    }

    /// <summary>
    /// Retrieves Fund balance events matching the provided query.
    /// </summary>
    public async Task<QueryPage<FundBalanceEvent>> GetAsync(
        FundBalanceEventQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await transactionQueryRepository.GetAsync(query.Start, query.End, cancellationToken);
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken);
        return await GetAsync(transactions, periods, query.Filter, query.Sort, query.Offset, query.Limit, cancellationToken);
    }

    /// <summary>
    /// Retrieves Fund balance events in the requested Accounting Period range.
    /// </summary>
    public async Task<FundBalanceEventAccountingPeriodRangeQueryResult> GetAsync(
        FundBalanceEventAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new FundBalanceEventAccountingPeriodRangeQueryResult(null, resolution.Failure);
        }

        IReadOnlyCollection<AccountingPeriod> periods = resolution.AccountingPeriods;
        IReadOnlyCollection<AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        IReadOnlyCollection<Transaction> transactions = await transactionQueryRepository.GetAsync(periodIds, cancellationToken);
        QueryPage<FundBalanceEvent> page = await GetAsync(
            transactions,
            periods,
            query.Filter,
            query.Sort,
            query.Offset,
            query.Limit,
            cancellationToken);
        return new FundBalanceEventAccountingPeriodRangeQueryResult(page, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Interprets and pages Fund balance events from the provided facts.
    /// </summary>
    private async Task<QueryPage<FundBalanceEvent>> GetAsync(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<AccountingPeriod> accountingPeriods,
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
        var historiesByFund = histories
            .GroupBy(history => history.Fund.Id)
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, historiesByFund))
            .Where(balanceEvent => Matches(balanceEvent.Fund, filter)).ToList();
        events = ProjectPendingEvents(events, transactions, historiesByFund);
        var allItems = Sort(events, sort).ToList();
        return new QueryPage<FundBalanceEvent>(
            allItems.Skip(offset).Take(limit ?? int.MaxValue).ToList(),
            allItems.Count);
    }

    /// <summary>
    /// Retrieves interpreted Fund balance events for a Transaction.
    /// </summary>
    private static IEnumerable<FundBalanceEvent> GetEvents(
        Transaction transaction,
        AccountingPeriod period,
        Dictionary<FundId, Fund> funds,
        IReadOnlyDictionary<FundId, List<FundBalanceHistory>> histories) => transaction switch
        {
            SpendingTransaction spending => spending.Destinations.SelectMany(destination => destination.FundAssignments)
                .Select(amount => Create(transaction, period, funds[amount.FundId], GetPostedDate(spending, amount.FundId), amount.Amount, BalanceEventType.Debit, ToParty(spending.Source.Account, null, null), spending.Destinations.Select(item => ToParty(item.Account, item.Location, item.Amount)).ToList(), histories)),
            IncomeTransaction income => income.Destinations.SelectMany(destination => destination.FundAssignments)
                .Select(amount => Create(transaction, period, funds[amount.FundId], GetPostedDate(income, amount.FundId), amount.Amount, BalanceEventType.Credit, ToParty(income.Source.Account, income.Source.Location, null), income.Destinations.Select(item => ToParty(item.Account, null, item.Amount)).ToList(), histories)),
            FundTransaction fund => new[] { Create(transaction, period, fund.Source.Fund, transaction.Date, transaction.Amount, BalanceEventType.Debit, new FundBalanceEventParty(fund.Source.Fund.Name, null), fund.Destinations.Select(item => new FundBalanceEventParty(item.Fund.Name, item.Amount)).ToList(), histories) }
                .Concat(fund.Destinations.Select(destination => Create(transaction, period, destination.Fund, transaction.Date, destination.Amount, BalanceEventType.Credit, new FundBalanceEventParty(fund.Source.Fund.Name, null), fund.Destinations.Select(item => new FundBalanceEventParty(item.Fund.Name, item.Amount)).ToList(), histories))),
            _ => [],
        };

    /// <summary>
    /// Creates an interpreted Fund balance event.
    /// </summary>
    private static FundBalanceEvent Create(
        Transaction transaction,
        AccountingPeriod period,
        Fund fund,
        DateOnly? postedDate,
        decimal amount,
        BalanceEventType type,
        FundBalanceEventParty source,
        IReadOnlyList<FundBalanceEventParty> destinations,
        IReadOnlyDictionary<FundId, List<FundBalanceHistory>> allHistories)
    {
        List<FundBalanceHistory> histories = allHistories.GetValueOrDefault(fund.Id) ?? [];
        FundBalanceHistory? current = histories.LastOrDefault(history => history.TransactionId == transaction.Id && history.Date == postedDate);
        int index = current == null ? -1 : histories.IndexOf(current);
        FundBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new FundBalanceEvent(
            period,
            transaction.Id,
            transaction.Date,
            transaction.Sequence,
            postedDate,
            postedDate == null ? null : current?.Sequence,
            type,
            amount,
            fund,
            source,
            destinations,
            ToBalance(fund, previous, fund.OnboardedBalance ?? 0),
            ToBalance(fund, current));
    }

    /// <summary>
    /// Creates a displayable source or destination party from its account and location.
    /// </summary>
    private static FundBalanceEventParty ToParty(Account? account, string? location, decimal? amount) => new(
        account?.Name ?? location ?? "Unspecified",
        amount);

    /// <summary>
    /// Creates a Fund balance from a Fund and its history.
    /// </summary>
    private static FundBalance ToBalance(Fund fund, FundBalanceHistory? history, decimal fallback = 0) => new(
        fund,
        history?.PostedBalance ?? fallback);

    /// <summary>
    /// Projects pending events from the final posted Fund balance in transaction order.
    /// </summary>
    private static List<FundBalanceEvent> ProjectPendingEvents(
        IReadOnlyCollection<FundBalanceEvent> events,
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyDictionary<FundId, List<FundBalanceHistory>> historiesByFund)
    {
        var transactionsById = transactions.ToDictionary(transaction => transaction.Id);
        var projected = events.ToList();
        foreach (IGrouping<FundId, FundBalanceEvent> fundEvents in events.Where(item => !item.IsPosted).GroupBy(item => item.Fund.Id))
        {
            Fund fund = fundEvents.First().Fund;
            FundBalance balance = new(fund, historiesByFund.GetValueOrDefault(fund.Id)?.LastOrDefault()?.PostedBalance ?? fund.OnboardedBalance ?? 0);
            foreach (IGrouping<TransactionId, FundBalanceEvent> transactionEvents in fundEvents
                .GroupBy(item => item.TransactionId)
                .OrderBy(group => transactionsById[group.Key].Date).ThenBy(group => transactionsById[group.Key].Sequence))
            {
                FundBalance previous = balance;
                balance = transactionsById[transactionEvents.Key].ApplyAsPostedToFundBalance(balance);
                foreach (FundBalanceEvent balanceEvent in transactionEvents)
                {
                    int index = projected.IndexOf(balanceEvent);
                    projected[index] = balanceEvent with { PreviousBalance = previous, NewBalance = balance };
                }
            }
        }
        return projected;
    }

    /// <summary>
    /// Gets the Fund's posting date from the Account side that affects it.
    /// </summary>
    private static DateOnly? GetPostedDate(Transaction transaction, FundId fundId) => transaction.GetAllAffectedAccountIds()
        .Where(accountId => transaction.GetAllAffectedFundIds(accountId).Contains(fundId))
        .Select(transaction.GetPostedDateForAccount).FirstOrDefault(date => date != null);

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
            FundBalanceEventSort.FundName => events.OrderBy(item => item.Fund.Name).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.FundNameDescending => events.OrderByDescending(item => item.Fund.Name).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.AccountingPeriod => events.OrderBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.AccountingPeriodDescending => events.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.Date => events.OrderBy(item => !item.IsPosted).ThenBy(item => item.IsPosted ? item.EventDate : item.TransactionDate).ThenBy(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.DateDescending => events.OrderByDescending(item => !item.IsPosted).ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate).ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
            FundBalanceEventSort.Counterparty => SortByText(events, GetCounterpartySortKey, false),
            FundBalanceEventSort.CounterpartyDescending => SortByText(events, GetCounterpartySortKey, true),
            FundBalanceEventSort.Source => SortByText(events, item => item.Source.DisplayName, false),
            FundBalanceEventSort.SourceDescending => SortByText(events, item => item.Source.DisplayName, true),
            FundBalanceEventSort.Destination => SortByText(events, GetDestinationSortKey, false),
            FundBalanceEventSort.DestinationDescending => SortByText(events, GetDestinationSortKey, true),
            _ => events.OrderByDescending(item => !item.IsPosted).ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate).ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence).ThenBy(item => item.TransactionId),
        };

    /// <summary>
    /// Sorts events by a displayed text value, putting events without a value last.
    /// </summary>
    private static IOrderedEnumerable<FundBalanceEvent> SortByText(
        IEnumerable<FundBalanceEvent> events,
        Func<FundBalanceEvent, string> getSortKey,
        bool descending) => descending
            ? events.OrderBy(item => string.IsNullOrWhiteSpace(getSortKey(item)))
                .ThenByDescending(getSortKey, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(item => item.EventDate)
                .ThenBy(item => item.TransactionId)
            : events.OrderBy(item => string.IsNullOrWhiteSpace(getSortKey(item)))
                .ThenBy(getSortKey, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(item => item.EventDate)
                .ThenBy(item => item.TransactionId);

    /// <summary>
    /// Gets the text displayed for transaction destinations in a balance-event list.
    /// </summary>
    private static string GetDestinationSortKey(FundBalanceEvent balanceEvent) => string.Join(
        ", ",
        balanceEvent.Destinations
            .Select(destination => destination.DisplayName)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the other party relevant to a balance event's debit or credit direction.
    /// </summary>
    private static string GetCounterpartySortKey(FundBalanceEvent balanceEvent) =>
        balanceEvent.Type == BalanceEventType.Debit
            ? GetDestinationSortKey(balanceEvent)
            : balanceEvent.Source.DisplayName;
}