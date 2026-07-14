namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update an account transaction.
/// </summary>
public sealed class UpdateAccountTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the account transaction.
    /// </summary>
    public required UpdateAccountTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the account transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateAccountTransactionDestinationModel> Destinations { get; init; }
}