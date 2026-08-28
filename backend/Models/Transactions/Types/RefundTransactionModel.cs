namespace Models.Transactions.Types;

/// <summary>
/// Model representing a refund transaction.
/// </summary>
public sealed class RefundTransactionModel : TransactionModel
{
    /// <summary>
    /// Sources for the refund transaction.
    /// </summary>
    public required IReadOnlyCollection<RefundTransactionSourceModel> Sources { get; init; }

    /// <summary>
    /// Destination for the refund transaction.
    /// </summary>
    public required RefundTransactionDestinationModel Destination { get; init; }
}
