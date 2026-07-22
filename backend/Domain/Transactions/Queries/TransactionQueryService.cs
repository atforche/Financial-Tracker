namespace Domain.Transactions.Queries;

/// <summary>
/// Service for querying interpreted Transaction details.
/// </summary>
public sealed class TransactionQueryService(ITransactionQueryRepository transactionQueryRepository)
{
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
}