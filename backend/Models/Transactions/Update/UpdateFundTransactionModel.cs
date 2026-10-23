using Models.Transactions.Create;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update a fund transaction.
/// </summary>
public sealed class UpdateFundTransactionModel : UpdateTransactionModel
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
