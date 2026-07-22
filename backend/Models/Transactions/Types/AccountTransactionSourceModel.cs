using Models.Accounts;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing the source of an account transaction response.
/// </summary>
public sealed class AccountTransactionSourceModel
{
    /// <summary>
    /// Optional account for the source.
    /// </summary>
    public AccountBalanceEventModel? Account { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public string? Location { get; init; }
}