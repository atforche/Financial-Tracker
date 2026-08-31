using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Refunds;
using Domain.Transactions.Spending;

namespace Domain.FundGoals.Queries;

/// <summary>
/// Service for querying interpreted Fund Goal balance events.
/// </summary>
public sealed class FundGoalBalanceEventQueryService(
    IFundGoalBalanceEventQueryRepository repository,
    ITransactionBalanceEventQueryRepository transactionQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IFundGoalRepository fundGoalRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves fully projected Fund Goal balance events for the requested Transactions.
    /// </summary>
    public async Task<IReadOnlyCollection<FundGoalBalanceEvent>> GetForTransactionsAsync(
        IReadOnlyCollection<Transaction> requestedTransactions,
        CancellationToken cancellationToken = default)
    {
        if (requestedTransactions.Count == 0)
        {
            return [];
        }
        IReadOnlyCollection<FundId> requestedFundIds = requestedTransactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null))
            .Distinct().ToList();
        IReadOnlyCollection<Transaction> pendingTransactions = await transactionQueryRepository.GetPendingForFundsAsync(requestedFundIds, cancellationToken);
        IReadOnlyCollection<Transaction> transactions = requestedTransactions.Concat(pendingTransactions).DistinctBy(transaction => transaction.Id).ToList();
        IReadOnlyCollection<FundId> fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null))
            .Distinct().ToList();
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        var periods = (await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken)).ToDictionary(period => period.Id);
        var funds = (await repository.GetFundsAsync(fundIds, cancellationToken)).ToDictionary(fund => fund.Id);
        IReadOnlyCollection<FundGoalTotalsHistory> histories = await repository.GetFundGoalHistoriesAsync(fundIds, cancellationToken);
        var historiesByFundAndPeriod = histories.GroupBy(history => (history.FundId, history.AccountingPeriodId))
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundGoalBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, historiesByFundAndPeriod))
            .Where(balanceEvent => requestedFundIds.Contains(balanceEvent.Fund.Id)).ToList();
        events = ProjectPendingEvents(events, transactions, historiesByFundAndPeriod);
        events = AddRemainingRegularAmountToAssign(events);
        var requestedIds = requestedTransactions.Select(transaction => transaction.Id).ToHashSet();
        return events.Where(balanceEvent => requestedIds.Contains(balanceEvent.TransactionId)).ToList();
    }

    /// <summary>
    /// Retrieves Fund Goal balance events matching the provided query.
    /// </summary>
    public async Task<QueryPage<FundGoalBalanceEvent>> GetAsync(
        FundGoalBalanceEventQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await transactionQueryRepository.GetAsync(query.Start, query.End, cancellationToken);
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken);
        return await GetAsync(transactions, periods, query.Filter, query.Sort, query.Offset, query.Limit, cancellationToken);
    }

    /// <summary>
    /// Retrieves Fund Goal balance events in the requested Accounting Period range.
    /// </summary>
    public async Task<FundGoalBalanceEventAccountingPeriodRangeQueryResult> GetAsync(
        FundGoalBalanceEventAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new FundGoalBalanceEventAccountingPeriodRangeQueryResult(null, resolution.Failure);
        }

        IReadOnlyCollection<AccountingPeriod> periods = resolution.AccountingPeriods;
        IReadOnlyCollection<AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        IReadOnlyCollection<Transaction> transactions = await transactionQueryRepository.GetAsync(periodIds, cancellationToken);
        QueryPage<FundGoalBalanceEvent> page = await GetAsync(
            transactions,
            periods,
            query.Filter,
            query.Sort,
            query.Offset,
            query.Limit,
            cancellationToken);
        return new FundGoalBalanceEventAccountingPeriodRangeQueryResult(page, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Interprets and pages Fund Goal balance events from the provided facts.
    /// </summary>
    private async Task<QueryPage<FundGoalBalanceEvent>> GetAsync(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<AccountingPeriod> accountingPeriods,
        FundGoalBalanceEventFilter filter,
        FundGoalBalanceEventSort sort,
        int offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        var periods = accountingPeriods.ToDictionary(period => period.Id);
        IReadOnlyCollection<FundId> fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null))
            .Distinct().ToList();
        var funds = (await repository.GetFundsAsync(fundIds, cancellationToken)).ToDictionary(fund => fund.Id);
        IReadOnlyCollection<FundGoalTotalsHistory> histories = await repository.GetFundGoalHistoriesAsync(fundIds, cancellationToken);
        Dictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundGoalTotalsHistory>> historiesByFundAndPeriod = histories
            .GroupBy(history => (history.FundId, history.AccountingPeriodId))
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundGoalBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, historiesByFundAndPeriod))
            .Where(balanceEvent => filter.FundIds.Count == 0 || filter.FundIds.Contains(balanceEvent.Fund.Id.Value)).ToList();
        events = ProjectPendingEvents(events, transactions, historiesByFundAndPeriod);
        events = AddRemainingRegularAmountToAssign(events);
        var allItems = Sort(events, sort).ToList();
        return new QueryPage<FundGoalBalanceEvent>(
            allItems.Skip(offset).Take(limit ?? int.MaxValue).ToList(),
            allItems.Count);
    }

    /// <summary>
    /// Enriches each event's historical totals with the remaining regular contribution amounts.
    /// </summary>
    private List<FundGoalBalanceEvent> AddRemainingRegularAmountToAssign(IReadOnlyCollection<FundGoalBalanceEvent> events)
    {
        var remainingByFundAndPeriod = new Dictionary<(FundId FundId, AccountingPeriodId PeriodId), decimal>();

        decimal GetTargetAmount(FundGoalBalanceEvent balanceEvent)
        {
            (FundId, AccountingPeriodId) key = (balanceEvent.Fund.Id, balanceEvent.AccountingPeriod.Id);
            if (remainingByFundAndPeriod.TryGetValue(key, out decimal targetAmount))
            {
                return targetAmount;
            }

            FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(
                balanceEvent.Fund.Id,
                balanceEvent.AccountingPeriod.Id);
            if (fundGoal == null)
            {
                remainingByFundAndPeriod[key] = 0;
                return 0;
            }

            AccountingPeriodBalanceHistory history = accountingPeriodBalanceHistoryRepository
                .GetForAccountingPeriod(balanceEvent.AccountingPeriod.Id);
            decimal currentBalance = history.FundBalances
                .SingleOrDefault(balance => balance.Fund.Id == balanceEvent.Fund.Id)
                ?.ClosingBalance ?? 0;
            targetAmount = FundGoalProgressService.CalculateRecommendedContribution(
                currentBalance,
                history.FundGoalTotals
                    .SingleOrDefault(totals => totals.Fund.Id == balanceEvent.Fund.Id)
                    ?.GetTotals().RegularAmountAssigned ?? 0,
                fundGoal.RegularContribution,
                fundGoal.MaximumEndingBalance);
            remainingByFundAndPeriod[key] = targetAmount;
            return targetAmount;
        }

        return events.Select(balanceEvent =>
        {
            decimal targetAmount = GetTargetAmount(balanceEvent);
            return balanceEvent with
            {
                PreviousTotals = AddRemainingAmount(balanceEvent.PreviousTotals, targetAmount),
                NewTotals = AddRemainingAmount(balanceEvent.NewTotals, targetAmount),
            };
        }).ToList();
    }

    /// <summary>
    /// Calculates remaining regular contribution amounts for a totals snapshot.
    /// </summary>
    private static FundGoalTotals AddRemainingAmount(
        FundGoalTotals totals,
        decimal targetAmount) => totals.WithRemainingRegularAmountToAssign(
            Math.Max(targetAmount - totals.RegularAmountAssigned, 0),
            Math.Max(targetAmount - totals.RegularAmountAssignedIncludingPending, 0));

    /// <summary>
    /// Retrieves interpreted Fund Goal balance events for a Transaction.
    /// </summary>
    private static IEnumerable<FundGoalBalanceEvent> GetEvents(
        Transaction transaction,
        AccountingPeriod period,
        Dictionary<FundId, Fund> funds,
        IReadOnlyDictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundGoalTotalsHistory>> histories) => transaction switch
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
                        new FundGoalBalanceEventParty(fund.Source.Fund.Name, null),
                        fund.Destinations.Select(item => new FundGoalBalanceEventParty(item.Fund.Name, item.Amount)).ToList(),
                        histories) }
                .Concat(fund.Destinations
                    .Select(destination => Create(
                        transaction,
                        period,
                        destination.Fund,
                        transaction.Date,
                        destination.Amount,
                        BalanceEventType.Credit,
                        new FundGoalBalanceEventParty(fund.Source.Fund.Name, null),
                        fund.Destinations.Select(item => new FundGoalBalanceEventParty(item.Fund.Name, item.Amount)).ToList(),
                        histories))),
            _ => [],
        };

    /// <summary>
    /// Creates an interpreted Fund Goal balance event.
    /// </summary>
    private static FundGoalBalanceEvent Create(
        Transaction transaction,
        AccountingPeriod period,
        Fund fund,
        DateOnly? postedDate,
        decimal amount,
        BalanceEventType type,
        FundGoalBalanceEventParty source,
        IReadOnlyList<FundGoalBalanceEventParty> destinations,
        IReadOnlyDictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundGoalTotalsHistory>> allHistories)
    {
        List<FundGoalTotalsHistory> histories = allHistories.GetValueOrDefault((fund.Id, transaction.AccountingPeriodId)) ?? [];
        FundGoalTotalsHistory? current = histories.SingleOrDefault(history => history.TransactionId == transaction.Id);
        int index = current == null ? -1 : histories.IndexOf(current);
        FundGoalTotalsHistory? previous = index > 0 ? histories[index - 1] : null;
        FundGoalTotals previousTotals = ToTotals(fund.Id, previous);
        FundGoalTotals newTotals = postedDate == null
            ? ToTotals(fund.Id, current)
            : transaction.ApplyAllPostedEffectsToFundGoalTotals(previousTotals);
        return new FundGoalBalanceEvent(
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
            previousTotals,
            newTotals);
    }

    /// <summary>
    /// Creates a displayable source or destination party from its account and location.
    /// </summary>
    private static FundGoalBalanceEventParty ToParty(Account? account, string? location, decimal? amount) => new(
        account?.Name ?? location ?? "Unspecified",
        amount);

    /// <summary>
    /// Creates Fund Goal totals from a history entry.
    /// </summary>
    private static FundGoalTotals ToTotals(FundId fundId, FundGoalTotalsHistory? history) => new(
        fundId,
        history?.AmountAssigned ?? 0,
        history?.AmountSpent ?? 0,
        history?.RegularAmountAssigned ?? 0,
        history?.AmountAssigned ?? 0,
        history?.RegularAmountAssigned ?? 0,
        history?.AmountSpent ?? 0);

    /// <summary>
    /// Projects pending events from final posted Fund Goal totals in transaction order.
    /// </summary>
    private static List<FundGoalBalanceEvent> ProjectPendingEvents(
        IReadOnlyCollection<FundGoalBalanceEvent> events,
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyDictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundGoalTotalsHistory>> histories)
    {
        var transactionsById = transactions.ToDictionary(transaction => transaction.Id);
        var projected = events.ToList();
        foreach (IGrouping<(FundId FundId, AccountingPeriodId PeriodId), FundGoalBalanceEvent> goalEvents in events.Where(item => !item.IsPosted)
            .GroupBy(item => (item.Fund.Id, item.AccountingPeriod.Id)))
        {
            FundGoalTotals totals = ToTotals(goalEvents.Key.FundId, histories.GetValueOrDefault(goalEvents.Key)?.LastOrDefault());
            foreach (IGrouping<TransactionId, FundGoalBalanceEvent> transactionEvents in goalEvents.GroupBy(item => item.TransactionId)
                .OrderBy(group => transactionsById[group.Key].Date).ThenBy(group => transactionsById[group.Key].Sequence))
            {
                FundGoalTotals previous = totals;
                totals = transactionsById[transactionEvents.Key].ApplyAsPostedToFundGoalTotals(totals);
                foreach (FundGoalBalanceEvent balanceEvent in transactionEvents)
                {
                    int index = projected.IndexOf(balanceEvent);
                    projected[index] = balanceEvent with { PreviousTotals = previous, NewTotals = totals };
                }
            }
        }
        return projected;
    }

    /// <summary>
    /// Sorts Fund Goal balance events by the provided sort order.
    /// </summary>
    private static IOrderedEnumerable<FundGoalBalanceEvent> Sort(
        IEnumerable<FundGoalBalanceEvent> events,
        FundGoalBalanceEventSort sort) => sort switch
        {
            FundGoalBalanceEventSort.FundName => events.OrderBy(item => item.Fund.Name).ThenByDescending(item => item.EventDate),
            FundGoalBalanceEventSort.FundNameDescending => events.OrderByDescending(item => item.Fund.Name).ThenByDescending(item => item.EventDate),
            FundGoalBalanceEventSort.Date => events.OrderBy(item => !item.IsPosted)
                .ThenBy(item => item.IsPosted ? item.EventDate : item.TransactionDate)
                .ThenBy(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
                .ThenBy(item => item.TransactionId),
            FundGoalBalanceEventSort.DateDescending => events.OrderByDescending(item => !item.IsPosted)
                .ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate)
                .ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
                .ThenBy(item => item.TransactionId),
            FundGoalBalanceEventSort.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.EventDate),
            FundGoalBalanceEventSort.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.EventDate),
            FundGoalBalanceEventSort.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.EventDate),
            FundGoalBalanceEventSort.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.EventDate),
            FundGoalBalanceEventSort.Counterparty => SortByText(events, GetCounterpartySortKey, false),
            FundGoalBalanceEventSort.CounterpartyDescending => SortByText(events, GetCounterpartySortKey, true),
            FundGoalBalanceEventSort.Source => SortByText(events, item => item.Source.DisplayName, false),
            FundGoalBalanceEventSort.SourceDescending => SortByText(events, item => item.Source.DisplayName, true),
            FundGoalBalanceEventSort.Destination => SortByText(events, GetDestinationSortKey, false),
            FundGoalBalanceEventSort.DestinationDescending => SortByText(events, GetDestinationSortKey, true),
            _ => events.OrderByDescending(item => !item.IsPosted).ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate)
                .ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
                .ThenBy(item => item.TransactionId),
        };

    /// <summary>
    /// Sorts events by a displayed text value, putting events without a value last.
    /// </summary>
    private static IOrderedEnumerable<FundGoalBalanceEvent> SortByText(
        IEnumerable<FundGoalBalanceEvent> events,
        Func<FundGoalBalanceEvent, string> getSortKey,
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
    private static string GetDestinationSortKey(FundGoalBalanceEvent balanceEvent) => string.Join(
        ", ",
        balanceEvent.Destinations
            .Select(destination => destination.DisplayName)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the other party relevant to a balance event's debit or credit direction.
    /// </summary>
    private static string GetCounterpartySortKey(FundGoalBalanceEvent balanceEvent) =>
        balanceEvent.Type == BalanceEventType.Debit
            ? GetDestinationSortKey(balanceEvent)
            : balanceEvent.Source.DisplayName;
}
