namespace Models.Transactions.Create;

/// <summary>
/// Model representing a request to create an account transaction.
/// </summary>
public sealed class CreateAccountTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Source for the account transaction.
    /// </summary>
    public required CreateAccountTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the account transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateAccountTransactionDestinationModel> Destinations { get; init; }
}
