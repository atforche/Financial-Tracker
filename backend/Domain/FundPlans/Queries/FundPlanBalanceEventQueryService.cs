using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Domain.FundPlans.Queries;

/// <summary>
/// Service for querying interpreted Fund Plan balance events.
/// </summary>
public sealed class FundPlanBalanceEventQueryService(
    IFundPlanBalanceEventQueryRepository repository,
    IAccountingPeriodQueryRepository accountingPeriodRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves Fund Plan balance events matching the provided query.
    /// </summary>
    public async Task<QueryPage<FundPlanBalanceEvent>> GetAsync(
        FundPlanBalanceEventQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await repository.GetTransactionsAsync(query.Start, query.End, cancellationToken);
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await accountingPeriodRepository.GetByIdsAsync(periodIds, cancellationToken);
        return await GetAsync(transactions, periods, query.Filter, query.Sort, query.Offset, query.Limit, cancellationToken);
    }

    /// <summary>
    /// Retrieves Fund Plan balance events in the requested Accounting Period range.
    /// </summary>
    public async Task<FundPlanBalanceEventAccountingPeriodRangeQueryResult> GetAsync(
        FundPlanBalanceEventAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new FundPlanBalanceEventAccountingPeriodRangeQueryResult(null, resolution.Failure);
        }

        IReadOnlyCollection<AccountingPeriod> periods = resolution.AccountingPeriods;
        IReadOnlyCollection<AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        IReadOnlyCollection<Transaction> transactions = await repository.GetTransactionsAsync(periodIds, cancellationToken);
        QueryPage<FundPlanBalanceEvent> page = await GetAsync(
            transactions,
            periods,
            query.Filter,
            query.Sort,
            query.Offset,
            query.Limit,
            cancellationToken);
        return new FundPlanBalanceEventAccountingPeriodRangeQueryResult(page, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Interprets and pages Fund Plan balance events from the provided facts.
    /// </summary>
    private async Task<QueryPage<FundPlanBalanceEvent>> GetAsync(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<AccountingPeriod> accountingPeriods,
        FundPlanBalanceEventFilter filter,
        FundPlanBalanceEventSort sort,
        int offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        var periods = accountingPeriods.ToDictionary(period => period.Id);
        IReadOnlyCollection<FundId> fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null))
            .Where(fundId => fundId != Fund.UnassignedFundId).Distinct().ToList();
        var funds = (await repository.GetFundsAsync(fundIds, cancellationToken)).ToDictionary(fund => fund.Id);
        IReadOnlyCollection<FundPlanTotalsHistory> histories = await repository.GetFundPlanHistoriesAsync(fundIds, cancellationToken);
        Dictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundPlanTotalsHistory>> historiesByFundAndPeriod = histories
            .GroupBy(history => (history.FundId, history.AccountingPeriodId))
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundPlanBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, historiesByFundAndPeriod))
            .Where(balanceEvent => filter.FundIds.Count == 0 || filter.FundIds.Contains(balanceEvent.Fund.Id.Value)).ToList();
        events = ProjectPendingEvents(events, transactions, historiesByFundAndPeriod);
        var allItems = Sort(events, sort).ToList();
        return new QueryPage<FundPlanBalanceEvent>(
            allItems.Skip(offset).Take(limit ?? int.MaxValue).ToList(),
            allItems.Count);
    }

    /// <summary>
    /// Retrieves interpreted Fund Plan balance events for a Transaction.
    /// </summary>
    private static IEnumerable<FundPlanBalanceEvent> GetEvents(
        Transaction transaction,
        AccountingPeriod period,
        Dictionary<FundId, Fund> funds,
        IReadOnlyDictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundPlanTotalsHistory>> histories) => transaction switch
        {
            SpendingTransaction spending => spending.Destinations.SelectMany(destination => destination.FundAssignments
                .Where(amount => amount.FundId != Fund.UnassignedFundId)
                .Select(amount => Create(transaction, period, funds[amount.FundId], destination.PostedDate, amount.Amount, BalanceEventType.Debit, histories))),
            IncomeTransaction income => income.Destinations.SelectMany(destination => destination.FundAssignments
                .Where(amount => amount.FundId != Fund.UnassignedFundId)
                .Select(amount => Create(transaction, period, funds[amount.FundId], destination.PostedDate, amount.Amount, BalanceEventType.Credit, histories))),
            FundTransaction fund => (fund.Source.Fund.Id == Fund.UnassignedFundId
                    ? Enumerable.Empty<FundPlanBalanceEvent>()
                    : new[] { Create(transaction, period, fund.Source.Fund, transaction.Date, transaction.Amount, BalanceEventType.Debit, histories) })
                .Concat(fund.Destinations.Where(destination => destination.Fund.Id != Fund.UnassignedFundId)
                    .Select(destination => Create(transaction, period, destination.Fund, transaction.Date, destination.Amount, BalanceEventType.Credit, histories))),
            _ => [],
        };

    /// <summary>
    /// Creates an interpreted Fund Plan balance event.
    /// </summary>
    private static FundPlanBalanceEvent Create(
        Transaction transaction,
        AccountingPeriod period,
        Fund fund,
        DateOnly? postedDate,
        decimal amount,
        BalanceEventType type,
        IReadOnlyDictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundPlanTotalsHistory>> allHistories)
    {
        List<FundPlanTotalsHistory> histories = allHistories.GetValueOrDefault((fund.Id, transaction.AccountingPeriodId)) ?? [];
        FundPlanTotalsHistory? current = histories.SingleOrDefault(history => history.TransactionId == transaction.Id);
        int index = current == null ? -1 : histories.IndexOf(current);
        FundPlanTotalsHistory? previous = index > 0 ? histories[index - 1] : null;
        return new FundPlanBalanceEvent(
            period,
            transaction.Id,
            transaction.Date,
            transaction.Sequence,
            postedDate,
            postedDate == null ? null : current?.Sequence,
            type,
            amount,
            fund,
            ToTotals(fund.Id, previous),
            ToTotals(fund.Id, current));
    }

    /// <summary>
    /// Creates Fund Plan totals from a history entry.
    /// </summary>
    private static FundPlanTotals ToTotals(FundId fundId, FundPlanTotalsHistory? history) => new(
        fundId,
        history?.AmountAssigned ?? 0,
        0,
        history?.AmountSpent ?? 0,
        0);

    /// <summary>
    /// Projects pending events from final posted Fund Plan totals in transaction order.
    /// </summary>
    private static List<FundPlanBalanceEvent> ProjectPendingEvents(
        IReadOnlyCollection<FundPlanBalanceEvent> events,
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyDictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundPlanTotalsHistory>> histories)
    {
        var transactionsById = transactions.ToDictionary(transaction => transaction.Id);
        var projected = events.ToList();
        foreach (IGrouping<(FundId FundId, AccountingPeriodId PeriodId), FundPlanBalanceEvent> planEvents in events.Where(item => !item.IsPosted)
            .GroupBy(item => (item.Fund.Id, item.AccountingPeriod.Id)))
        {
            FundPlanTotals totals = ToTotals(planEvents.Key.FundId, histories.GetValueOrDefault(planEvents.Key)?.LastOrDefault());
            foreach (IGrouping<TransactionId, FundPlanBalanceEvent> transactionEvents in planEvents.GroupBy(item => item.TransactionId)
                .OrderBy(group => transactionsById[group.Key].Date).ThenBy(group => transactionsById[group.Key].Sequence))
            {
                FundPlanTotals previous = totals;
                totals = transactionsById[transactionEvents.Key].ApplyAsPostedToFundPlanTotals(totals);
                foreach (FundPlanBalanceEvent balanceEvent in transactionEvents)
                {
                    int index = projected.IndexOf(balanceEvent);
                    projected[index] = balanceEvent with { PreviousTotals = previous, NewTotals = totals };
                }
            }
        }
        return projected;
    }

    /// <summary>
    /// Sorts Fund Plan balance events by the provided sort order.
    /// </summary>
    private static IOrderedEnumerable<FundPlanBalanceEvent> Sort(
        IEnumerable<FundPlanBalanceEvent> events,
        FundPlanBalanceEventSort sort) => sort switch
        {
            FundPlanBalanceEventSort.FundName => events.OrderBy(item => item.Fund.Name).ThenByDescending(item => item.EventDate),
            FundPlanBalanceEventSort.FundNameDescending => events.OrderByDescending(item => item.Fund.Name).ThenByDescending(item => item.EventDate),
            FundPlanBalanceEventSort.Date => events.OrderBy(item => !item.IsPosted).ThenBy(item => item.IsPosted ? item.EventDate : item.TransactionDate).ThenBy(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence).ThenBy(item => item.TransactionId),
            FundPlanBalanceEventSort.DateDescending => events.OrderByDescending(item => !item.IsPosted).ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate).ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence).ThenBy(item => item.TransactionId),
            FundPlanBalanceEventSort.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.EventDate),
            FundPlanBalanceEventSort.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.EventDate),
            FundPlanBalanceEventSort.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.EventDate),
            FundPlanBalanceEventSort.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.EventDate),
            _ => events.OrderByDescending(item => !item.IsPosted).ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate).ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence).ThenBy(item => item.TransactionId),
        };
}