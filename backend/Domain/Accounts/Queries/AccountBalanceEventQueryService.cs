using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.BalanceEvents;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Refunds;
using Domain.Transactions.Spending;

namespace Domain.Accounts.Queries;

/// <summary>
/// Service for querying interpreted Account balance events.
/// </summary>
public sealed class AccountBalanceEventQueryService(
    IAccountQueryRepository accountQueryRepository,
    IAccountBalanceEventQueryRepository repository,
    ITransactionBalanceEventQueryRepository transactionQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves fully projected Account balance events for the requested Transactions.
    /// </summary>
    public async Task<IReadOnlyCollection<AccountBalanceEvent>> GetForTransactionsAsync(
        IReadOnlyCollection<Transaction> requestedTransactions,
        CancellationToken cancellationToken = default)
    {
        if (requestedTransactions.Count == 0)
        {
            return [];
        }
        IReadOnlyCollection<AccountId> accountIds = requestedTransactions
            .SelectMany(transaction => transaction.GetAllAffectedAccountIds()).Distinct().ToList();
        IReadOnlyCollection<Transaction> pendingTransactions = await transactionQueryRepository.GetPendingForAccountsAsync(accountIds, cancellationToken);
        IReadOnlyCollection<Transaction> transactions = requestedTransactions.Concat(pendingTransactions)
            .DistinctBy(transaction => transaction.Id).ToList();
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        var periods = (await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken)).ToDictionary(period => period.Id);
        IReadOnlyCollection<AccountBalanceHistory> histories = await repository.GetAccountHistoriesAsync(accountIds, cancellationToken);
        var historiesByAccount = histories.GroupBy(history => history.Account.Id)
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<AccountBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], historiesByAccount))
            .Where(balanceEvent => accountIds.Contains(balanceEvent.Account.Id)).ToList();
        events = ProjectPendingEvents(events, transactions, historiesByAccount);
        var requestedIds = requestedTransactions.Select(transaction => transaction.Id).ToHashSet();
        return events.Where(balanceEvent => requestedIds.Contains(balanceEvent.TransactionId)).ToList();
    }

    /// <summary>
    /// Retrieves balance events for the requested Account.
    /// </summary>
    public async Task<QueryPage<AccountBalanceEvent>> GetAsync(
        AccountBalanceEventAccountQuery query,
        CancellationToken cancellationToken = default)
    {
        var accountId = new AccountId(query.AccountId);
        Account? account = await accountQueryRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            return new QueryPage<AccountBalanceEvent>([], 0);
        }
        IReadOnlyCollection<Transaction> recentTransactions = await transactionQueryRepository.GetForAccountAsync(
            accountId,
            query.Start,
            query.End,
            cancellationToken);
        IReadOnlyCollection<Transaction> pendingTransactions = await transactionQueryRepository.GetPendingForAccountsAsync(
            [accountId],
            cancellationToken);
        IReadOnlyCollection<Transaction> transactions = recentTransactions.Concat(pendingTransactions).DistinctBy(transaction => transaction.Id).ToList();
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken);
        IReadOnlyCollection<AccountBalanceHistory> histories = await repository.GetAccountHistoriesAsync([accountId], cancellationToken);
        return GetAccountEvents(
            recentTransactions,
            pendingTransactions,
            periods.ToDictionary(period => period.Id),
            account,
            histories.OrderBy(history => history.Date).ThenBy(history => history.Sequence).ToList(),
            query.Sort,
            query.Offset,
            query.Limit);
    }

    /// <summary>
    /// Builds a recent posted ledger and the current pending projection for one Account.
    /// </summary>
    private static QueryPage<AccountBalanceEvent> GetAccountEvents(
        IReadOnlyCollection<Transaction> recentTransactions,
        IReadOnlyCollection<Transaction> pendingTransactions,
        Dictionary<AccountingPeriodId, AccountingPeriod> periods,
        Account account,
        List<AccountBalanceHistory> histories,
        AccountBalanceEventSort sort,
        int offset,
        int? limit)
    {
        AccountId accountId = account.Id;
        var historiesByAccount = new Dictionary<AccountId, List<AccountBalanceHistory>> { [accountId] = histories };
        IReadOnlyCollection<Transaction> transactions = recentTransactions.Concat(pendingTransactions)
            .DistinctBy(transaction => transaction.Id).ToList();
        IReadOnlyCollection<AccountBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], historiesByAccount))
            .Where(balanceEvent => balanceEvent.Account.Id == accountId).ToList();
        var allItems = Sort(ProjectPendingEvents(events, transactions, historiesByAccount), sort).ToList();
        return new QueryPage<AccountBalanceEvent>(
            allItems.Skip(offset).Take(limit ?? int.MaxValue).ToList(),
            allItems.Count);
    }

    /// <summary>
    /// Retrieves Account balance events matching the provided query.
    /// </summary>
    public async Task<QueryPage<AccountBalanceEvent>> GetAsync(AccountBalanceEventQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await transactionQueryRepository.GetAsync(query.Start, query.End, cancellationToken);
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken);
        return await GetAsync(transactions, periods, query.Filter, query.Sort, query.Offset, query.Limit, null, cancellationToken);
    }

    /// <summary>
    /// Retrieves Account balance events in the requested Accounting Period range.
    /// </summary>
    public async Task<AccountBalanceEventAccountingPeriodRangeQueryResult> GetAsync(
        AccountBalanceEventAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountBalanceEventAccountingPeriodRangeQueryResult(null, resolution.Failure);
        }

        IReadOnlyCollection<AccountingPeriod> periods = resolution.AccountingPeriods;
        IReadOnlyCollection<AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        IReadOnlyCollection<Transaction> transactions = await transactionQueryRepository.GetAsync(periodIds, cancellationToken);
        QueryPage<AccountBalanceEvent> page = await GetAsync(
            transactions,
            periods,
            query.Filter,
            query.Sort,
            query.Offset,
            query.Limit,
            null,
            cancellationToken);
        return new AccountBalanceEventAccountingPeriodRangeQueryResult(page, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Interprets and pages Account balance events from the provided facts.
    /// </summary>
    private async Task<QueryPage<AccountBalanceEvent>> GetAsync(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<AccountingPeriod> accountingPeriods,
        AccountFilter filter,
        AccountBalanceEventSort sort,
        int offset,
        int? limit,
        AccountId? accountId,
        CancellationToken cancellationToken)
    {
        var periods = accountingPeriods.ToDictionary(period => period.Id);
        IReadOnlyCollection<AccountId> accountIds = transactions.SelectMany(transaction => transaction.GetAllAffectedAccountIds()).Distinct().ToList();
        IReadOnlyCollection<AccountBalanceHistory> histories = await repository.GetAccountHistoriesAsync(accountIds, cancellationToken);
        var historiesByAccount = histories
            .GroupBy(history => history.Account.Id)
            .ToDictionary(group => group.Key, group => group.ToList());
        IEnumerable<AccountBalanceEvent> events = transactions.SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], historiesByAccount))
            .Where(balanceEvent => Matches(balanceEvent.Account, filter))
            .Where(balanceEvent => accountId == null || balanceEvent.Account.Id == accountId);
        var allItems = Sort(events, sort).ToList();
        return new QueryPage<AccountBalanceEvent>(
            allItems.Skip(offset).Take(limit ?? int.MaxValue).ToList(),
            allItems.Count);
    }

    /// <summary>
    /// Retrieves interpreted Account balance events for a Transaction.
    /// </summary>
    private static IEnumerable<AccountBalanceEvent> GetEvents(
        Transaction transaction,
        AccountingPeriod period,
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> histories) => transaction switch
        {
            SpendingTransaction spending => GetSpendingEvents(transaction, period, spending, histories),
            IncomeTransaction income => GetIncomeEvents(transaction, period, income, histories),
            AccountTransaction account => GetAccountEvents(transaction, period, account, histories),
            RefundTransaction refund => GetRefundEvents(transaction, period, refund, histories),
            _ => [],
        };

    private static IEnumerable<AccountBalanceEvent> GetRefundEvents(
        Transaction transaction,
        AccountingPeriod period,
        RefundTransaction refund,
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> histories)
    {
        var sourceParties = refund.Sources.Select(source => ToParty(source.Account, source.Location?.Name, source.Amount)).ToList();
        IEnumerable<AccountBalanceEvent> sourceEvents = refund.Sources.Where(source => source.Account != null).Select(source => Create(
            transaction, period, source.Account!, source.PostedDate, source.Amount, BalanceEventType.Debit,
            ToParty(source.Account, source.Location?.Name, null), [ToParty(refund.Destination.Account, null, transaction.Amount)], histories));
        return sourceEvents.Append(Create(transaction, period, refund.Destination.Account, refund.Destination.PostedDate,
            transaction.Amount, BalanceEventType.Credit, ToParty(refund.Destination.Account, null, null), sourceParties, histories));
    }

    private static IEnumerable<AccountBalanceEvent> GetSpendingEvents(
        Transaction transaction,
        AccountingPeriod period,
        SpendingTransaction spending,
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> histories)
    {
        var parties = spending.Destinations
            .Select(destination => ToParty(
                destination.Account,
                destination.Location?.Name,
                destination.Amount))
            .ToList();
        AccountBalanceEvent source = Create(
            transaction,
            period,
            spending.Source.Account,
            spending.Source.PostedDate,
            transaction.Amount,
            BalanceEventType.Debit,
            ToParty(spending.Source.Account, null, null),
            parties,
            histories);
        return new[] { source }.Concat(spending.Destinations
            .Where(destination => destination.Account != null)
            .Select(destination => Create(
                transaction,
                period,
                destination.Account!,
                destination.PostedDate,
                destination.Amount,
                BalanceEventType.Credit,
                ToParty(spending.Source.Account, null, null),
                parties,
                histories)));
    }

    private static IEnumerable<AccountBalanceEvent> GetIncomeEvents(
        Transaction transaction,
        AccountingPeriod period,
        IncomeTransaction income,
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> histories)
    {
        var parties = income.Destinations
            .Select(destination => ToParty(
                destination.Account,
                null,
                destination.Amount))
            .ToList();
        IEnumerable<AccountBalanceEvent> sourceEvents = income.Source.Account == null
            ? []
            : new[] { Create(
                transaction,
                period,
                income.Source.Account,
                income.Source.PostedDate,
                transaction.Amount,
                BalanceEventType.Debit,
                ToParty(income.Source.Account, income.Source.Location?.Name, null),
                parties,
                histories) };
        return sourceEvents.Concat(income.Destinations.Select(destination => Create(
            transaction,
            period,
            destination.Account,
            destination.PostedDate,
            destination.Amount,
            BalanceEventType.Credit,
            ToParty(income.Source.Account, income.Source.Location?.Name, null),
            parties,
            histories)));
    }

    private static IEnumerable<AccountBalanceEvent> GetAccountEvents(
        Transaction transaction,
        AccountingPeriod period,
        AccountTransaction account,
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> histories)
    {
        var parties = account.Destinations
            .Select(destination => ToParty(
                destination.Account,
                destination.Location?.Name,
                destination.Amount))
            .ToList();
        IEnumerable<AccountBalanceEvent> sourceEvents = account.Source.Account == null
            ? []
            : new[] { Create(
                transaction,
                period,
                account.Source.Account,
                account.Source.PostedDate,
                transaction.Amount,
                BalanceEventType.Debit,
                ToParty(account.Source.Account, account.Source.Location?.Name, null),
                parties,
                histories) };
        return sourceEvents.Concat(account.Destinations
            .Where(destination => destination.Account != null)
            .Select(destination => Create(
                transaction,
                period,
                destination.Account!,
                destination.PostedDate,
                destination.Amount,
                BalanceEventType.Credit,
                ToParty(account.Source.Account, account.Source.Location?.Name, null),
                parties,
                histories)));
    }

    /// <summary>
    /// Creates an interpreted Account balance event for a Transaction and Account.
    /// </summary>
    private static AccountBalanceEvent Create(
        Transaction transaction,
        AccountingPeriod period,
        Account account,
        DateOnly? postedDate,
        decimal amount,
        BalanceEventType type,
        AccountBalanceEventParty source,
        IReadOnlyList<AccountBalanceEventParty> destinations,
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> allHistories)
    {
        List<AccountBalanceHistory> histories = allHistories.GetValueOrDefault(account.Id) ?? [];
        AccountBalanceHistory? current = histories.LastOrDefault(history => history.TransactionId == transaction.Id && history.Date == (postedDate ?? transaction.Date));
        int index = current == null ? -1 : histories.IndexOf(current);
        AccountBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new AccountBalanceEvent(
            period,
            transaction.Id,
            transaction.Description,
            transaction.Date,
            transaction.Sequence,
            postedDate,
            postedDate == null ? null : current?.Sequence,
            type,
            amount,
            account,
            source,
            destinations,
            ToBalance(account, previous, account.OnboardedBalance ?? 0),
            ToBalance(account, current));
    }

    /// <summary>
    /// Creates a displayable source or destination party from its account and location.
    /// </summary>
    private static AccountBalanceEventParty ToParty(Account? account, string? location, decimal? amount) => new(
        account?.Name ?? location ?? "Unspecified",
        amount);

    /// <summary>
    /// Creates an Account balance from an Account and its history.
    /// </summary>
    private static AccountBalance ToBalance(Account account, AccountBalanceHistory? history, decimal fallback = 0) => new(
        account,
        history?.PostedBalance ?? fallback);

    /// <summary>
    /// Projects pending events from the final posted Account balance in transaction order.
    /// </summary>
    private static List<AccountBalanceEvent> ProjectPendingEvents(
        IReadOnlyCollection<AccountBalanceEvent> events,
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> historiesByAccount)
    {
        var transactionsById = transactions.ToDictionary(transaction => transaction.Id);
        var projected = events.ToList();
        foreach (IGrouping<AccountId, AccountBalanceEvent> accountEvents in events.Where(item => !item.IsPosted).GroupBy(item => item.Account.Id))
        {
            Account account = accountEvents.First().Account;
            AccountBalance balance = new(account, historiesByAccount.GetValueOrDefault(account.Id)?.LastOrDefault()?.PostedBalance ?? account.OnboardedBalance ?? 0);
            foreach (IGrouping<TransactionId, AccountBalanceEvent> transactionEvents in accountEvents
                .GroupBy(item => item.TransactionId)
                .OrderBy(group => transactionsById[group.Key].Date).ThenBy(group => transactionsById[group.Key].Sequence))
            {
                AccountBalance previous = balance;
                balance = transactionsById[transactionEvents.Key].ApplyAsPostedToAccountBalance(balance);
                foreach (AccountBalanceEvent balanceEvent in transactionEvents)
                {
                    int index = projected.IndexOf(balanceEvent);
                    projected[index] = balanceEvent with { PreviousBalance = previous, NewBalance = balance };
                }
            }
        }
        return projected;
    }

    /// <summary>
    /// Determines whether an Account matches the provided query criteria.
    /// </summary>
    private static bool Matches(Account account, AccountFilter filter) =>
        (string.IsNullOrWhiteSpace(filter.NameSearch)
            || account.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase)
            || (account.FinancialInstitution?.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase) ?? false))
        && (filter.Names.Count == 0 || filter.Names.Contains(account.Name))
        && (filter.Types.Count == 0 || filter.Types.Contains(account.Type));

    /// <summary>
    /// Sorts a collection of Account balance events by the provided sort order.
    /// </summary>
    private static IOrderedEnumerable<AccountBalanceEvent> Sort(IEnumerable<AccountBalanceEvent> events, AccountBalanceEventSort sort) => sort switch
    {
        AccountBalanceEventSort.AccountName => events.OrderBy(item => item.Account.Name).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AccountNameDescending => events.OrderByDescending(item => item.Account.Name).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AccountingPeriod => events.OrderBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AccountingPeriodDescending => events.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.Date => events.OrderBy(item => !item.IsPosted)
            .ThenBy(item => item.IsPosted ? item.EventDate : item.TransactionDate)
            .ThenBy(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
            .ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.DateDescending => events.OrderByDescending(item => !item.IsPosted)
            .ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate)
            .ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
            .ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.EventDate).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.Counterparty => SortByText(events, GetCounterpartySortKey, false),
        AccountBalanceEventSort.CounterpartyDescending => SortByText(events, GetCounterpartySortKey, true),
        _ => events.OrderByDescending(item => !item.IsPosted)
            .ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate)
            .ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
            .ThenBy(item => item.TransactionId),
    };

    /// <summary>
    /// Sorts events by a displayed text value, putting events without a value last.
    /// </summary>
    private static IOrderedEnumerable<AccountBalanceEvent> SortByText(
        IEnumerable<AccountBalanceEvent> events,
        Func<AccountBalanceEvent, string> getSortKey,
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
    private static string GetDestinationSortKey(AccountBalanceEvent balanceEvent) => string.Join(
        ", ",
        balanceEvent.Destinations
            .Select(destination => destination.DisplayName)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the other party relevant to a balance event's debit or credit direction.
    /// </summary>
    private static string GetCounterpartySortKey(AccountBalanceEvent balanceEvent) =>
        balanceEvent.Type == BalanceEventType.Debit
            ? GetDestinationSortKey(balanceEvent)
            : balanceEvent.Source.DisplayName;
}
