namespace Models.Funds;

/// <summary>
/// Model representing a Fund row within the trends response.
/// </summary>
public class FundTrendsFundModel
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