using Models.Accounts;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing the source of a spending transaction response.
/// </summary>
public sealed class SpendingTransactionSourceModel
{
    /// <summary>
    /// Account for the source.
    /// </summary>
    public required AccountBalanceEventModel Account { get; init; }
}