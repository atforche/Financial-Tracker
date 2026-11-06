namespace Models.BalanceEvents;

/// <summary>
/// Model representing balance events and their totals
/// </summary>
public sealed class BalanceEventsWithTotalsModel<TEvent>
{
    /// <summary>
    /// Matching balance events
    /// </summary>
    public required IReadOnlyCollection<TEvent> Events { get; init; }

    /// <summary>
    /// Number of matching balance events before pagination
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Totals of the matching posted balance events
    /// </summary>
    public required BalanceEventTotalsModel Totals { get; init; }
}
