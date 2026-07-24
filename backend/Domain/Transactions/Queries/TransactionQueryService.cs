using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts.Queries;
using Domain.FundPlans.Queries;
using Domain.Funds.Queries;

namespace Domain.Transactions.Queries;

/// <summary>
/// Service for querying interpreted Transaction details.
/// </summary>
public sealed class TransactionQueryService(
    ITransactionQueryRepository transactionQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService,
    AccountBalanceEventQueryService accountBalanceEventQueryService,
    FundBalanceEventQueryService fundBalanceEventQueryService,
    FundPlanBalanceEventQueryService fundPlanBalanceEventQueryService)
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
        IReadOnlyCollection<AccountBalanceEvent> accountEvents = await accountBalanceEventQueryService.GetForTransactionsAsync(
            facts.Transactions.Items,
            cancellationToken);
        IReadOnlyCollection<FundBalanceEvent> fundEvents = await fundBalanceEventQueryService.GetForTransactionsAsync(
            facts.Transactions.Items,
            cancellationToken);
        IReadOnlyCollection<FundPlanBalanceEvent> fundPlanEvents = await fundPlanBalanceEventQueryService.GetForTransactionsAsync(
            facts.Transactions.Items,
            cancellationToken);
        IReadOnlyCollection<TransactionDetails> items = facts.Transactions.Items.Select(transaction => new TransactionDetails(
            new TransactionDetailsFacts(
                transaction,
                periods[transaction.AccountingPeriodId]),
                accountEvents.Where(balanceEvent => balanceEvent.TransactionId == transaction.Id).ToList(),
                fundEvents.Where(balanceEvent => balanceEvent.TransactionId == transaction.Id).ToList(),
                fundPlanEvents.Where(balanceEvent => balanceEvent.TransactionId == transaction.Id).ToList())).ToList();
        return new QueryPage<TransactionDetails>(items, facts.Transactions.TotalCount);
    }

    /// <summary>
    /// Resolves balance events for one Transaction detail response.
    /// </summary>
    private async Task<TransactionDetails> ToDetailsAsync(
        TransactionDetailsFacts facts,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<AccountBalanceEvent> accountEvents = await accountBalanceEventQueryService.GetForTransactionsAsync([facts.Transaction], cancellationToken);
        IReadOnlyCollection<FundBalanceEvent> fundEvents = await fundBalanceEventQueryService.GetForTransactionsAsync([facts.Transaction], cancellationToken);
        IReadOnlyCollection<FundPlanBalanceEvent> fundPlanEvents = await fundPlanBalanceEventQueryService.GetForTransactionsAsync([facts.Transaction], cancellationToken);
        return new TransactionDetails(facts, accountEvents, fundEvents, fundPlanEvents);
    }
}