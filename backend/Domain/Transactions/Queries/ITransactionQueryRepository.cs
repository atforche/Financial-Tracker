namespace Domain.Transactions.Queries;

/// <summary>
/// Defines persisted fact retrieval for Transaction queries.
/// </summary>
public interface ITransactionQueryRepository
{
    /// <summary>
    /// Retrieves a Transaction page and all facts required to interpret it.
    /// </summary>
    Task<TransactionQueryFacts> GetAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all facts required to interpret a Transaction, or null when it does not exist.
    /// </summary>
    Task<TransactionDetailsFacts?> GetDetailsByIdAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken = default);
}