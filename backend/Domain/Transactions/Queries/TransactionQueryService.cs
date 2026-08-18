using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts.Queries;
using Domain.FundGoals.Queries;
using Domain.Funds.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Transactions.Queries;

/// <summary>
/// Service for querying interpreted Transaction details.
/// </summary>
public sealed class TransactionQueryService(
    ITransactionQueryRepository transactionQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService,
    IServiceScopeFactory serviceScopeFactory)
{
    /// <summary>
    /// Retrieves the Transaction with the specified ID, or null when it does not exist.
    /// </summary>
    public Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
        transactionQueryRepository.GetByIdAsync(new TransactionId(transactionId), cancellationToken);

    /// <summary>
    /// Retrieves interpreted Transactions matching the provided query.
    /// </summary>
    public async Task<QueryPage<TransactionDetails>> GetAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        TransactionQueryFacts facts = await transactionQueryRepository.GetAsync(query, cancellationToken);
        return await ToPageAsync(facts, cancellationToken);
    }

    /// <summary>
    /// Retrieves interpreted Transactions and metadata for a date range.
    /// </summary>
    public async Task<TransactionDateRange> GetDateRangeAsync(
        TransactionDateRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        TransactionDateRangeFacts facts = await transactionQueryRepository.GetDateRangeAsync(query, cancellationToken);
        return new TransactionDateRange(
            await ToPageAsync(facts.QueryFacts, cancellationToken),
            facts.AvailableAccountNames,
            facts.AvailableFundNames,
            facts.TransactionTypes,
            facts.LocationCashFlow,
            query.Offset,
            query.Limit);
    }

    /// <summary>
    /// Retrieves interpreted Transactions and metadata for an Accounting Period range.
    /// </summary>
    public async Task<TransactionAccountingPeriodRangeQueryResult> GetAccountingPeriodRangeAsync(
        TransactionAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new TransactionAccountingPeriodRangeQueryResult(null, resolution.Failure);
        }

        IReadOnlyCollection<AccountingPeriod> periods = resolution.AccountingPeriods;
        IReadOnlyCollection<AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        TransactionAccountingPeriodRangeFacts facts = await transactionQueryRepository.GetAccountingPeriodRangeAsync(
            query,
            periodIds,
            cancellationToken);
        var range = new TransactionAccountingPeriodRange(
            await ToPageAsync(facts.QueryFacts, cancellationToken),
            facts.AvailableAccountNames,
            facts.AvailableFundNames,
            facts.TransactionTypes,
            facts.LocationCashFlow,
            query.Offset,
            query.Limit);
        return new TransactionAccountingPeriodRangeQueryResult(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Retrieves interpreted Transaction details by ID, or null when no Transaction exists.
    /// </summary>
    public async Task<TransactionDetails?> GetDetailsByIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        TransactionDetailsFacts? facts = await transactionQueryRepository.GetDetailsByIdAsync(
            new TransactionId(transactionId),
            cancellationToken);
        return facts == null ? null : await ToDetailsAsync(facts, cancellationToken);
    }

    /// <summary>
    /// Interprets a factual Transaction page.
    /// </summary>
    private async Task<QueryPage<TransactionDetails>> ToPageAsync(
        TransactionQueryFacts facts,
        CancellationToken cancellationToken)
    {
        var periods = facts.AccountingPeriods.ToDictionary(period => period.Id);
        Task<IReadOnlyCollection<AccountBalanceEvent>> accountEventsTask = GetAccountEventsAsync(
            facts.Transactions.Items,
            cancellationToken);
        Task<IReadOnlyCollection<FundBalanceEvent>> fundEventsTask = GetFundEventsAsync(
            facts.Transactions.Items,
            cancellationToken);
        Task<IReadOnlyCollection<FundGoalBalanceEvent>> fundGoalEventsTask = GetFundGoalEventsAsync(
            facts.Transactions.Items,
            cancellationToken);
        await Task.WhenAll(accountEventsTask, fundEventsTask, fundGoalEventsTask);
        IReadOnlyCollection<AccountBalanceEvent> accountEvents = await accountEventsTask;
        IReadOnlyCollection<FundBalanceEvent> fundEvents = await fundEventsTask;
        IReadOnlyCollection<FundGoalBalanceEvent> fundGoalEvents = await fundGoalEventsTask;
        IReadOnlyCollection<TransactionDetails> items = facts.Transactions.Items.Select(transaction => new TransactionDetails(
            new TransactionDetailsFacts(
                transaction,
                periods[transaction.AccountingPeriodId]),
                accountEvents.Where(balanceEvent => balanceEvent.TransactionId == transaction.Id).ToList(),
                fundEvents.Where(balanceEvent => balanceEvent.TransactionId == transaction.Id).ToList(),
                fundGoalEvents.Where(balanceEvent => balanceEvent.TransactionId == transaction.Id).ToList())).ToList();
        return new QueryPage<TransactionDetails>(items, facts.Transactions.TotalCount);
    }

    /// <summary>
    /// Resolves balance events for one Transaction detail response.
    /// </summary>
    private async Task<TransactionDetails> ToDetailsAsync(
        TransactionDetailsFacts facts,
        CancellationToken cancellationToken)
    {
        Task<IReadOnlyCollection<AccountBalanceEvent>> accountEventsTask = GetAccountEventsAsync(
            [facts.Transaction],
            cancellationToken);
        Task<IReadOnlyCollection<FundBalanceEvent>> fundEventsTask = GetFundEventsAsync(
            [facts.Transaction],
            cancellationToken);
        Task<IReadOnlyCollection<FundGoalBalanceEvent>> fundGoalEventsTask = GetFundGoalEventsAsync(
            [facts.Transaction],
            cancellationToken);
        await Task.WhenAll(accountEventsTask, fundEventsTask, fundGoalEventsTask);
        return new TransactionDetails(
            facts,
            await accountEventsTask,
            await fundEventsTask,
            await fundGoalEventsTask);
    }

    /// <summary>
    /// Retrieves balance events for Transactions in a new database context.
    /// </summary>
    private async Task<IReadOnlyCollection<AccountBalanceEvent>> GetAccountEventsAsync(
        IReadOnlyCollection<Transaction> transactions,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
        AccountBalanceEventQueryService service = scope.ServiceProvider
            .GetRequiredService<AccountBalanceEventQueryService>();
        return await service.GetForTransactionsAsync(transactions, cancellationToken);
    }

    /// <summary>
    /// Retrieves balance events for Transactions in a new database context.
    /// </summary>
    private async Task<IReadOnlyCollection<FundBalanceEvent>> GetFundEventsAsync(
        IReadOnlyCollection<Transaction> transactions,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
        FundBalanceEventQueryService service = scope.ServiceProvider
            .GetRequiredService<FundBalanceEventQueryService>();
        return await service.GetForTransactionsAsync(transactions, cancellationToken);
    }

    /// <summary>
    /// Retrieves balance events for Transactions in a new database context.
    /// </summary>
    private async Task<IReadOnlyCollection<FundGoalBalanceEvent>> GetFundGoalEventsAsync(
        IReadOnlyCollection<Transaction> transactions,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
        FundGoalBalanceEventQueryService service = scope.ServiceProvider
            .GetRequiredService<FundGoalBalanceEventQueryService>();
        return await service.GetForTransactionsAsync(transactions, cancellationToken);
    }
}
