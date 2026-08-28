namespace Domain.Transactions.Refunds;

/// <summary>
/// Request to create a refund transaction.
/// </summary>
public record CreateRefundTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// The sources of the refund transaction.
    /// </summary>
    public required IReadOnlyCollection<RefundTransactionSource> Sources { get; init; }

    /// <summary>
    /// The destination of the refund transaction.
    /// </summary>
    public required RefundTransactionDestination Destination { get; init; }
}
