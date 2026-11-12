using Models.Accounts;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing a balance adjustment. Its amount is a signed change to the account's balance as it is displayed.
/// </summary>
public sealed class BalanceAdjustmentModel : TransactionModel
{
    /// <summary>
    /// Account whose balance is adjusted.
    /// </summary>
    public required AccountBalanceEventModel Account { get; init; }
}
