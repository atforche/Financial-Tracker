using Models.Transactions.Create;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update a spending transaction.
/// </summary>
public sealed class UpdateSpendingTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the spending transaction.
    /// </summary>
    public required CreateSpendingTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the spending transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateSpendingTransactionDestinationModel> Destinations { get; init; }
}
