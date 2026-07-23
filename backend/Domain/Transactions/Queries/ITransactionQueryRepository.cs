using Domain.AccountingPeriods;

namespace Domain.Transactions.Queries;

/// <summary>
/// Defines persisted fact retrieval for Transaction queries.
/// </summary>
public interface ITransactionQueryRepository
{
    /// <summary>
    /// Retrieves the Transaction with the specified ID, or null when it does not exist.
    /// </summary>
    Task<Transaction?> GetByIdAsync(TransactionId transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Transaction page and all facts required to interpret it.
    /// </summary>
    Task<TransactionQueryFacts> GetAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transaction date-range facts and supporting metadata.
    /// </summary>
    Task<TransactionDateRangeFacts> GetDateRangeAsync(
        TransactionDateRangeQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transaction facts for the provided Accounting Period IDs.
    /// </summary>
    Task<TransactionAccountingPeriodRangeFacts> GetAccountingPeriodRangeAsync(
        TransactionAccountingPeriodRangeQuery query,
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all facts required to interpret a Transaction, or null when it does not exist.
    /// </summary>
    Task<TransactionDetailsFacts?> GetDetailsByIdAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken = default);
}