namespace Domain.Transactions.Refunds;

/// <summary>
/// Record representing a request to update a <see cref="RefundTransaction"/>.
/// </summary>
public record UpdateRefundTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Sources for this Refund Transaction.
    /// </summary>
    public required IReadOnlyCollection<RefundTransactionSource> Sources { get; init; }

    /// <summary>
    /// Destination for this Refund Transaction.
    /// </summary>
    public required RefundTransactionDestination Destination { get; init; }
}
