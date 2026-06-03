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

    /// <summary>
    /// Total balance across assigned Funds.
    /// </summary>
    public required decimal AssignedBalance { get; init; }

    /// <summary>
    /// Total balance across unassigned Funds.
    /// </summary>
    public required decimal UnassignedBalance { get; init; }
}