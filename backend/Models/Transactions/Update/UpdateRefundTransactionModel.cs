using Models.Transactions.Create;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update a refund transaction.
/// </summary>
public sealed class UpdateRefundTransactionModel : UpdateTransactionModel
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
