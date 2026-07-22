using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;

namespace Domain.Transactions.Queries;

/// <summary>
/// Service for querying interpreted Transaction details.
/// </summary>
public sealed class TransactionQueryService(
    ITransactionQueryRepository transactionQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves interpreted Transactions matching the provided query.
    /// </summary>
    public async Task<QueryPage<TransactionDetails>> GetAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        TransactionQueryFacts facts = await transactionQueryRepository.GetAsync(query, cancellationToken);
        return ToPage(facts);
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
            ToPage(facts.QueryFacts),
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
            ToPage(facts.QueryFacts),
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
    public async Task<TransactionDetails?> GetByIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        TransactionDetailsFacts? facts = await transactionQueryRepository.GetDetailsByIdAsync(
            new TransactionId(transactionId),
            cancellationToken);
        return facts == null ? null : new TransactionDetails(facts);
    }

    /// <summary>
    /// Interprets a factual Transaction page.
    /// </summary>
    private static QueryPage<TransactionDetails> ToPage(TransactionQueryFacts facts)
    {
        var periods = facts.AccountingPeriods.ToDictionary(period => period.Id);
        IReadOnlyCollection<TransactionDetails> items = facts.Transactions.Items.Select(transaction => new TransactionDetails(
            new TransactionDetailsFacts(
                transaction,
                periods[transaction.AccountingPeriodId],
                facts.Funds,
                facts.AccountHistories,
                facts.FundHistories,
                facts.FundPlanHistories))).ToList();
        return new QueryPage<TransactionDetails>(items, facts.Transactions.TotalCount);
    }
}