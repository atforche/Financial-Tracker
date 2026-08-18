namespace Models.Transactions.Create;

/// <summary>
/// Model representing a request to create a fund transaction.
/// </summary>
public sealed class CreateFundTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Source for the fund transaction.
    /// </summary>
    public required CreateFundTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the fund transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateFundTransactionDestinationModel> Destinations { get; init; }
}
