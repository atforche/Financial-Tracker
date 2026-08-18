namespace Models.Funds;

/// <summary>
/// Model representing a Fund with a balance range.
/// </summary>
public class FundWithBalanceRangeModel : FundModel
{
    /// <summary>
    /// Balance at the beginning of the requested range.
    /// </summary>
    public required decimal StartingBalance { get; init; }

    /// <summary>
    /// Balance at the end of the requested range.
    /// </summary>
    public required decimal EndingBalance { get; init; }
}
