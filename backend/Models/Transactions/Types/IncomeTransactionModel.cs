namespace Models.Transactions.Types;

/// <summary>
/// Model representing an income transaction.
/// </summary>
public sealed class IncomeTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the income transaction.
    /// </summary>
    public required IncomeTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Total tracked amount for the income transaction.
    /// </summary>
    public required decimal TrackedAmount { get; init; }

    /// <summary>
    /// Destinations for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeTransactionDestinationModel> Destinations { get; init; }
}
