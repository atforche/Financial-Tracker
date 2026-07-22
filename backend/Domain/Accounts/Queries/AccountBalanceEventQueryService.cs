using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.BalanceEvents;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Domain.Accounts.Queries;

/// <summary>
/// Service for querying interpreted Account balance events.
/// </summary>
public sealed class AccountBalanceEventQueryService(
    IAccountBalanceEventQueryRepository repository,
    IAccountingPeriodQueryRepository accountingPeriodRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves Account balance events matching the provided query.
    /// </summary>
    public async Task<QueryPage<AccountBalanceEvent>> GetAsync(AccountBalanceEventQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await repository.GetTransactionsAsync(query.Start, query.End, cancellationToken);
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken);
        return await GetAsync(transactions, periods, query.Filter, query.Sort, query.Offset, query.Limit, cancellationToken);
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
        IReadOnlyCollection<Transaction> transactions = await repository.GetTransactionsAsync(periodIds, cancellationToken);
        QueryPage<AccountBalanceEvent> page = await GetAsync(
            transactions,
            periods,
            query.Filter,
            query.Sort,
            query.Offset,
            query.Limit,
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
        CancellationToken cancellationToken)
    {
        var periods = accountingPeriods.ToDictionary(period => period.Id);
        IReadOnlyCollection<AccountId> accountIds = transactions.SelectMany(transaction => transaction.GetAllAffectedAccountIds()).Distinct().ToList();
        IReadOnlyCollection<AccountBalanceHistory> histories = await repository.GetAccountHistoriesAsync(accountIds, cancellationToken);
        var historiesByAccount = histories
            .GroupBy(history => history.Account.Id)
            .ToDictionary(group => group.Key, group => group.ToList());
        IEnumerable<AccountBalanceEvent> events = transactions.SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], historiesByAccount))
            .Where(balanceEvent => Matches(balanceEvent.Account, filter));
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
            SpendingTransaction spending => new[] { Create(transaction, period, spending.Source.Account, spending.Source.PostedDate, transaction.Amount, BalanceEventType.Debit, histories) }
                .Concat(spending.Destinations.Where(destination => destination.Account != null)
                    .Select(destination => Create(transaction, period, destination.Account!, destination.PostedDate, destination.Amount, BalanceEventType.Credit, histories))),
            IncomeTransaction income => (income.Source.Account == null
                    ? Enumerable.Empty<AccountBalanceEvent>()
                    : new[] { Create(transaction, period, income.Source.Account, income.Source.PostedDate, transaction.Amount, BalanceEventType.Debit, histories) })
                .Concat(income.Destinations.Select(destination => Create(transaction, period, destination.Account, destination.PostedDate, destination.Amount, BalanceEventType.Credit, histories))),
            AccountTransaction account => (account.Source.Account == null
                    ? Enumerable.Empty<AccountBalanceEvent>()
                    : new[] { Create(transaction, period, account.Source.Account, account.Source.PostedDate, transaction.Amount, BalanceEventType.Debit, histories) })
                .Concat(account.Destinations.Where(destination => destination.Account != null)
                    .Select(destination => Create(transaction, period, destination.Account!, destination.PostedDate, destination.Amount, BalanceEventType.Credit, histories))),
            _ => [],
        };

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
        IReadOnlyDictionary<AccountId, List<AccountBalanceHistory>> allHistories)
    {
        List<AccountBalanceHistory> histories = allHistories.GetValueOrDefault(account.Id) ?? [];
        AccountBalanceHistory? current = histories.LastOrDefault(history => history.TransactionId == transaction.Id && history.Date == (postedDate ?? transaction.Date));
        int index = current == null ? -1 : histories.IndexOf(current);
        AccountBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new AccountBalanceEvent(
            period,
            transaction.Id,
            postedDate,
            type,
            amount,
            account,
            ToBalance(account, previous, account.OnboardedBalance ?? 0),
            ToBalance(account, current));
    }

    /// <summary>
    /// Creates an Account balance from an Account and its history.
    /// </summary>
    private static AccountBalance ToBalance(Account account, AccountBalanceHistory? history, decimal fallback = 0) => new(
        account,
        history?.PostedBalance ?? fallback,
        history?.PendingDebitAmount ?? 0,
        history?.PendingCreditAmount ?? 0);

    /// <summary>
    /// Determines whether an Account matches the provided query criteria.
    /// </summary>
    private static bool Matches(Account account, AccountFilter filter) =>
        (string.IsNullOrWhiteSpace(filter.NameSearch) || account.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase))
        && (filter.Names.Count == 0 || filter.Names.Contains(account.Name))
        && (filter.Types.Count == 0 || filter.Types.Contains(account.Type));

    /// <summary>
    /// Sorts a collection of Account balance events by the provided sort order.
    /// </summary>
    private static IOrderedEnumerable<AccountBalanceEvent> Sort(IEnumerable<AccountBalanceEvent> events, AccountBalanceEventSort sort) => sort switch
    {
        AccountBalanceEventSort.AccountName => events.OrderBy(item => item.Account.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AccountNameDescending => events.OrderByDescending(item => item.Account.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AccountingPeriod => events.OrderBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AccountingPeriodDescending => events.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.Date => events.OrderBy(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.DateDescending => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSort.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        _ => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
    };
}