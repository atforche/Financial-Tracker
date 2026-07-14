namespace Models.Transactions.Types;

/// <summary>
/// Model representing a fund transaction.
/// </summary>
public sealed class FundTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the fund transaction.
    /// </summary>
    public required FundTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the fund transaction.
    /// </summary>
    public required IReadOnlyCollection<FundTransactionDestinationModel> Destinations { get; init; }
}