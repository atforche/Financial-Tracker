namespace Models.Transactions.Types;

/// <summary>
/// Model representing an account transaction.
/// </summary>
public sealed class AccountTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the account transaction.
    /// </summary>
    public required AccountTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the account transaction.
    /// </summary>
    public required IReadOnlyCollection<AccountTransactionDestinationModel> Destinations { get; init; }
}