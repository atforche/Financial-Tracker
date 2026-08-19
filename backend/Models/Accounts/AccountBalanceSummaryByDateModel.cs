namespace Models.Accounts;

/// <summary>
/// Model representing a summary of account balances for a specific date.
/// </summary>
public class AccountBalanceSummaryByDateModel : AccountBalanceSummaryModel
{
    /// <summary>
    /// Date for this summary.
    /// </summary>
    public required DateOnly Date { get; init; }
}
