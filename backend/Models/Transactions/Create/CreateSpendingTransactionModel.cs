namespace Models.Transactions.Create;

/// <summary>
/// Model representing a request to create a spending transaction.
/// </summary>
public sealed class CreateSpendingTransactionModel : CreateTransactionModel
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
