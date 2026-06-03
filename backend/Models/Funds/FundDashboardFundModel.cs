namespace Models.Funds;

/// <summary>
/// Model representing a Fund row within the dashboard response.
/// </summary>
public class FundDashboardFundModel
{
    /// <summary>
    /// ID for the Fund.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Fund.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Balance at the beginning of the requested range.
    /// </summary>
    public required decimal StartingBalance { get; init; }

    /// <summary>
    /// Balance at the end of the requested range.
    /// </summary>
    public required decimal EndingBalance { get; init; }
}