namespace Models.Transactions.Types;

/// <summary>
/// Model representing a spending transaction.
/// </summary>
public sealed class SpendingTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the spending transaction.
    /// </summary>
    public required SpendingTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the spending transaction.
    /// </summary>
    public required IReadOnlyCollection<SpendingTransactionDestinationModel> Destinations { get; init; }
}
