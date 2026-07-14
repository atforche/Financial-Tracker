namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update a fund transaction.
/// </summary>
public sealed class UpdateFundTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the fund transaction.
    /// </summary>
    public required UpdateFundTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the fund transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateFundTransactionDestinationModel> Destinations { get; init; }
}