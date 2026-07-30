namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update an income transaction.
/// </summary>
public sealed class UpdateIncomeTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the income transaction.
    /// </summary>
    public required UpdateIncomeTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeTransactionDestinationModel> Destinations { get; init; }
}