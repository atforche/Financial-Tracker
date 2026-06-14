namespace Models.Accounts;

/// <summary>
/// Model representing summary balances for Accounts.
/// </summary>
public class AccountSummaryModel
{
    /// <summary>
    /// Sum of the posted balances for all Accounts.
    /// </summary>
    public required decimal TotalBalance { get; init; }

    /// <summary>
    /// Sum of the posted balances for tracked Accounts.
    /// </summary>
    public required decimal TotalTrackedBalance { get; init; }

    /// <summary>
    /// Sum of the posted balances for untracked Accounts.
    /// </summary>
    public required decimal TotalUntrackedBalance { get; init; }

    /// <summary>
    /// Breakdown of total posted balances by Account Type.
    /// </summary>
    public required IReadOnlyCollection<AccountTypeBalanceModel> BalanceByAccountType { get; init; }
}