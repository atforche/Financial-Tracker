namespace Models.Transactions.Create;

/// <summary>
/// Model representing a request to create a refund transaction.
/// </summary>
public sealed class CreateRefundTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Sources for the refund transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateRefundTransactionSourceModel> Sources { get; init; }

    /// <summary>
    /// Destination for the refund transaction.
    /// </summary>
    public required CreateRefundTransactionDestinationModel Destination { get; init; }
}
