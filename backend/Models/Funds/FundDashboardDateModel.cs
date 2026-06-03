namespace Models.Funds;

/// <summary>
/// Model representing a Fund balance on a specific date on the dashboard.
/// </summary>
public class FundDashboardDateModel
{
    /// <summary>
    /// Date for the balance.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Balance for the Fund on the specified date.
    /// </summary>
    public required decimal Balance { get; init; }
}