namespace Models.Funds;

/// <summary>
/// Model representing top-level dashboard balances for a specific date.
/// </summary>
public class FundDashboardDateSummaryModel
{
    /// <summary>
    /// Date for this summary.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Total balance across all matching Funds.
    /// </summary>
    public required decimal TotalBalance { get; init; }
}