using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Transactions;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Refunds;
using Domain.Transactions.Spending;

namespace Domain.Funds.Queries;

/// <summary>
/// Service for querying interpreted Fund balance events.
/// </summary>
public sealed class FundBalanceEventQueryService(
    IFundBalanceEventQueryRepository repository,
    ITransactionBalanceEventQueryRepository transactionQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodRepository)
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
        IReadOnlyCollection<FundId>? requestedFundIds = query.Filter.Ids.Count == 0
            ? null
            : query.Filter.Ids.Select(id => new FundId(id)).ToList();
        IReadOnlyCollection<Transaction> postedTransactions = await transactionQueryRepository.GetForFundPostingsAsync(query.Start, query.End, requestedFundIds, cancellationToken);
        IReadOnlyCollection<Transaction> pendingTransactions = query.IncludePending
            ? await transactionQueryRepository.GetPendingForFundsAsync(requestedFundIds, cancellationToken)
            : [];
        IReadOnlyCollection<Transaction> transactions = postedTransactions.Concat(pendingTransactions)
            .DistinctBy(transaction => transaction.Id).ToList();
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken);
        return await GetAsync(transactions, periods, query.Filter, query.Offset, query.Limit, cancellationToken,
            query.Start, query.End, true, query.IncludePending);
    }

    /// <summary>
    /// Interprets and pages Fund balance events from the provided facts.
    /// </summary>
    private async Task<QueryPage<FundBalanceEvent>> GetAsync(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<AccountingPeriod> accountingPeriods,
        FundFilter filter,
        int offset,
        int? limit,
        CancellationToken cancellationToken,
        DateOnly? postingStart = null,
        DateOnly? postingEnd = null,
        bool filterByPostingDate = false,
        bool includePending = false)
    {
        var periods = accountingPeriods.ToDictionary(period => period.Id);
        IReadOnlyCollection<FundId> fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null)).Distinct().ToList();
        var funds = (await repository.GetFundsAsync(fundIds, cancellationToken)).ToDictionary(fund => fund.Id);
        IReadOnlyCollection<FundId> reportedFundIds = funds.Values.Where(fund => Matches(fund, filter)).Select(fund => fund.Id).ToList();
        IReadOnlyCollection<FundBalanceHistory> histories = await repository.GetFundHistoriesAsync(reportedFundIds, cancellationToken);
        var historiesByFund = histories
            .GroupBy(history => history.Fund.Id)
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, historiesByFund))
            .Where(balanceEvent => Matches(balanceEvent.Fund, filter)).ToList();
        var allItems = Sort(events.Where(item => (includePending && !item.IsPosted)
            || (item.IsPosted
                && (!filterByPostingDate || item.EventDate.HasValue)
                && (postingStart == null || item.EventDate >= postingStart)
                && (postingEnd == null || item.EventDate <= postingEnd)))).ToList();
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
            SpendingTransaction spending => spending.Destinations.SelectMany(destination => destination.FundAssignments
                .Select(amount => Create(
                    transaction,
                    period,
                    funds[amount.FundId],
                    destination.Account == null ? spending.Source.PostedDate : destination.PostedDate,
                    amount.Amount,
                    BalanceEventType.Debit,
                    ToParty(spending.Source.Account, null, null),
                    spending.Destinations
                        .Select(item => ToParty(item.Account, item.Location?.Name, item.Amount))
                        .ToList(),
                    histories))),
            IncomeTransaction income => income.Destinations.SelectMany(destination => destination.FundAssignments
                .Select(amount => Create(
                    transaction,
                    period,
                    funds[amount.FundId],
                    destination.PostedDate,
                    amount.Amount,
                    BalanceEventType.Credit,
                    ToParty(destination.Account, null, destination.Amount),
                    [ToParty(destination.Account, null, destination.Amount)],
                    histories))),
            RefundTransaction refund => refund.Sources.SelectMany(source => source.FundAssignments
                .Select(amount => Create(
                    transaction,
                    period,
                    funds[amount.FundId],
                    source.Account == null ? refund.Destination.PostedDate : source.PostedDate,
                    amount.Amount,
                    BalanceEventType.Credit,
                    ToParty(source.Account, source.Location?.Name, source.Amount),
                    [ToParty(refund.Destination.Account, null, transaction.Amount)],
                    histories))),
            FundTransaction fund => new[] { Create(
                transaction,
                period,
                fund.Source.Fund,
                transaction.Date,
                transaction.Amount,
                BalanceEventType.Debit,
                new FundBalanceEventParty(fund.Source.Fund.Name, null),
                fund.Destinations.Select(item => new FundBalanceEventParty(item.Fund.Name, item.Amount)).ToList(),
                histories) }
                .Concat(fund.Destinations.Select(destination => Create(
                    transaction,
                    period,
                    destination.Fund,
                    transaction.Date,
                    destination.Amount,
                    BalanceEventType.Credit,
                    new FundBalanceEventParty(fund.Source.Fund.Name, null),
                    fund.Destinations.Select(item => new FundBalanceEventParty(item.Fund.Name, item.Amount)).ToList(),
                    histories))),
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
            transaction.Description,
            transaction.Date,
            transaction.Sequence,
            postedDate,
            postedDate == null ? null : current?.Sequence,
            type,
            amount,
            fund,
            source,
            destinations,
            postedDate == null ? null : ToBalance(fund, previous, fund.OnboardedBalance ?? 0),
            postedDate == null ? null : ToBalance(fund, current));
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
    /// Determines whether a Fund matches the provided filter.
    /// </summary>
    private static bool Matches(Fund fund, FundFilter filter) =>
        (string.IsNullOrWhiteSpace(filter.NameSearch) || fund.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase))
        && (filter.Names.Count == 0 || filter.Names.Contains(fund.Name))
        && (filter.Ids.Count == 0 || filter.Ids.Contains(fund.Id.Value));

    /// <summary>
    /// Orders Fund balance events newest first, with pending events ahead of posted ones.
    /// </summary>
    private static IOrderedEnumerable<FundBalanceEvent> Sort(IEnumerable<FundBalanceEvent> events) => events
        .OrderByDescending(item => !item.IsPosted)
        .ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate)
        .ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
        .ThenBy(item => item.TransactionId);
}
