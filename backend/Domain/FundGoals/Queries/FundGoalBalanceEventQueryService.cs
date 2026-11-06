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
        IReadOnlyCollection<FundGoalTotalsHistory> histories = await repository.GetFundGoalHistoriesAsync(fundIds, null, cancellationToken);
        var historiesByFundAndPeriod = histories.GroupBy(history => (history.FundId, history.AccountingPeriodId))
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundGoalBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periods[transaction.AccountingPeriodId], funds, historiesByFundAndPeriod))
            .Where(balanceEvent => requestedFundIds.Contains(balanceEvent.Fund.Id)).ToList();
        events = AddRemainingAmountToAssignToExpectedContribution(events);
        var requestedIds = requestedTransactions.Select(transaction => transaction.Id).ToHashSet();
        return events.Where(balanceEvent => requestedIds.Contains(balanceEvent.TransactionId)).ToList();
    }

    /// <summary>
    /// Retrieves Fund Goal balance events in an Accounting Period range for totaling, narrowing to any requested Funds in
    /// the query and leaving out the remaining contribution amounts, which cost a balance lookup per Fund and period.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalBalanceEvent>>> GetForTotalsAsync(
        FundGoalBalanceEventAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalBalanceEvent>>(null, resolution.Failure);
        }

        IReadOnlyCollection<AccountingPeriod> periods = resolution.AccountingPeriods;
        IReadOnlyCollection<AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        var fundIds = query.Filter.FundIds.Select(id => new FundId(id)).ToList();
        IReadOnlyCollection<Transaction> transactions = fundIds.Count == 0
            ? await transactionQueryRepository.GetAsync(periodIds, cancellationToken)
            : await transactionQueryRepository.GetForFundsAsync(periodIds, fundIds, cancellationToken);
        var funds = (await repository.GetFundsAsync(GetFundIds(transactions), cancellationToken)).ToDictionary(fund => fund.Id);
        var periodsById = periods.ToDictionary(period => period.Id);
        IReadOnlyCollection<FundGoalBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, periodsById[transaction.AccountingPeriodId], funds, new Dictionary<(FundId, AccountingPeriodId), List<FundGoalTotalsHistory>>()))
            .Where(balanceEvent => query.Filter.FundIds.Count == 0 || query.Filter.FundIds.Contains(balanceEvent.Fund.Id.Value)).ToList();
        return new AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalBalanceEvent>>(events, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Retrieves one Fund's goal balance events in an Accounting Period, newest first, from its already loaded goal and
    /// period balances.
    /// </summary>
    public async Task<IReadOnlyCollection<FundGoalBalanceEvent>> GetForFundAsync(
        AccountingPeriod accountingPeriod,
        FundId fundId,
        FundGoal? fundGoal,
        AccountingPeriodBalanceHistory balanceHistory,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await transactionQueryRepository.GetForFundsAsync([accountingPeriod.Id], [fundId], cancellationToken);
        var funds = (await repository.GetFundsAsync(GetFundIds(transactions), cancellationToken)).ToDictionary(fund => fund.Id);
        IReadOnlyCollection<FundGoalTotalsHistory> histories = await repository.GetFundGoalHistoriesAsync([fundId], [accountingPeriod.Id], cancellationToken);
        Dictionary<(FundId FundId, AccountingPeriodId PeriodId), List<FundGoalTotalsHistory>> historiesByFundAndPeriod = histories
            .GroupBy(history => (history.FundId, history.AccountingPeriodId))
            .ToDictionary(group => group.Key, group => group.ToList());
        IReadOnlyCollection<FundGoalBalanceEvent> events = transactions
            .SelectMany(transaction => GetEvents(transaction, accountingPeriod, funds, historiesByFundAndPeriod))
            .Where(balanceEvent => balanceEvent.Fund.Id == fundId).ToList();
        return AddRemainingAmountToAssignToExpectedContribution(events, (fundGoal, balanceHistory))
            .OrderByDescending(item => !item.IsPosted)
            .ThenByDescending(item => item.IsPosted ? item.EventDate : item.TransactionDate)
            .ThenByDescending(item => item.IsPosted ? item.EventDateSequence : item.TransactionSequence)
            .ThenBy(item => item.TransactionId)
            .ToList();
    }

    /// <summary>
    /// Gets the Funds affected by the provided Transactions.
    /// </summary>
    private static List<FundId> GetFundIds(IReadOnlyCollection<Transaction> transactions) =>
        transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null)).Distinct().ToList();

    /// <summary>
    /// Enriches each event's historical totals with the remaining expected monthly contribution amounts.
    /// </summary>
    private List<FundGoalBalanceEvent> AddRemainingAmountToAssignToExpectedContribution(
        IReadOnlyCollection<FundGoalBalanceEvent> events,
        (FundGoal? Goal, AccountingPeriodBalanceHistory History)? loaded = null)
    {
        var remainingByFundAndPeriod = new Dictionary<(FundId FundId, AccountingPeriodId PeriodId), decimal>();

        decimal GetExpectedAmount(FundGoalBalanceEvent balanceEvent)
        {
            (FundId, AccountingPeriodId) key = (balanceEvent.Fund.Id, balanceEvent.AccountingPeriod.Id);
            if (remainingByFundAndPeriod.TryGetValue(key, out decimal expectedAmount))
            {
                return expectedAmount;
            }

            FundGoal? fundGoal = loaded != null
                ? loaded.Value.Goal
                : fundGoalRepository.GetByFundAndAccountingPeriod(balanceEvent.Fund.Id, balanceEvent.AccountingPeriod.Id);
            if (fundGoal == null)
            {
                remainingByFundAndPeriod[key] = 0;
                return 0;
            }

            AccountingPeriodBalanceHistory history = loaded?.History
                ?? accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(balanceEvent.AccountingPeriod.Id);
            decimal currentEndingBalance = history.FundBalances
                .SingleOrDefault(balance => balance.Fund.Id == balanceEvent.Fund.Id)
                ?.ClosingBalance ?? 0;
            expectedAmount = FundGoalProgressService.CalculateExpectedContribution(
                currentEndingBalance,
                history.FundGoalTotals
                    .SingleOrDefault(totals => totals.Fund.Id == balanceEvent.Fund.Id)
                    ?.GetTotals().AmountAssignedToExpectedContribution ?? 0,
                fundGoal.PlannedMonthlyContribution,
                fundGoal.MaximumEndingBalance,
                fundGoal.AllowExpectedContributionAboveMaximum);
            remainingByFundAndPeriod[key] = expectedAmount;
            return expectedAmount;
        }

        return events.Select(balanceEvent =>
        {
            decimal expectedAmount = GetExpectedAmount(balanceEvent);
            return balanceEvent with
            {
                PreviousTotals = AddRemainingAmount(balanceEvent.PreviousTotals, expectedAmount),
                NewTotals = AddRemainingAmount(balanceEvent.NewTotals, expectedAmount),
            };
        }).ToList();
    }

    /// <summary>
    /// Calculates remaining expected monthly contribution amounts for a totals snapshot.
    /// </summary>
    private static FundGoalTotals? AddRemainingAmount(
        FundGoalTotals? totals,
        decimal expectedAmount) => totals?.WithRemainingAmountToAssignToExpectedContribution(
            Math.Max(expectedAmount - totals.AmountAssignedToExpectedContribution, 0));

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
        FundGoalTotals? previousTotals = postedDate == null ? null : ToTotals(fund.Id, previous);
        FundGoalTotals? newTotals = previousTotals == null
            ? null
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
        history?.AmountAssignedToExpectedContribution ?? 0);
}
