using Models.BalanceEvents;

namespace Models.Funds;

/// <summary>
/// Model representing a Fund's balance events and their totals
/// </summary>
public sealed class FundBalanceEventsWithTotalsModel
{
    /// <summary>
    /// Matching balance events
    /// </summary>
    public required IReadOnlyCollection<FundBalanceEventModel> Events { get; init; }

    /// <summary>
    /// Totals of the matching posted balance events
    /// </summary>
    public required BalanceEventTotalsModel Totals { get; init; }
}
