using Models.Transactions.Create;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update an account transaction.
/// </summary>
public sealed class UpdateAccountTransactionModel : UpdateTransactionModel
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
