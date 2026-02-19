using Domain.Funds;

namespace Data.Funds;

/// <summary>
/// Model representing information about a Fund required for sorting
/// </summary>
internal sealed class FundSortModel
{
    /// <summary>
    /// Fund
    /// </summary>
    public required Fund Fund { get; init; }

    /// <summary>
    /// Posted balance for the Fund
    /// </summary>
    public required decimal PostedBalance { get; init; }
}