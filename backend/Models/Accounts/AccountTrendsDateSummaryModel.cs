namespace Models.Accounts;

/// <summary>
/// Model representing top-level trends balances for a specific date.
/// </summary>
public class AccountTrendsDateSummaryModel
{
    /// <summary>
    /// Date for this summary.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Total balance across all matching Accounts.
    /// </summary>
    public required decimal TotalBalance { get; init; }

    /// <summary>
    /// Total balance across tracked Accounts.
    /// </summary>
    public required decimal TrackedBalance { get; init; }

    /// <summary>
    /// Total balance across untracked Accounts.
    /// </summary>
    public required decimal UntrackedBalance { get; init; }

    /// <summary>
    /// Total balances grouped by Account Type.
    /// </summary>
    public required IReadOnlyCollection<AccountTypeBalanceModel> BalanceByAccountType { get; init; }
}