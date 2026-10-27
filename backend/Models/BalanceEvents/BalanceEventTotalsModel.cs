namespace Models.BalanceEvents;

/// <summary>
/// Totals of posted balance events in the requested range.
/// </summary>
public sealed class BalanceEventTotalsModel
{
    /// <summary>
    /// Sum of posted credits.
    /// </summary>
    public required decimal TotalInflow { get; init; }

    /// <summary>
    /// Sum of posted debits.
    /// </summary>
    public required decimal TotalOutflow { get; init; }

    /// <summary>
    /// Posted balance before the first matching posted event.
    /// </summary>
    public decimal? OpeningBalance { get; init; }

    /// <summary>
    /// Last matching posted balance on each event date, or at the end of each day, week, or month when a date range
    /// is requested.
    /// </summary>
    public IReadOnlyCollection<BalanceEventDateBalanceModel>? Dates { get; init; }

    /// <summary>
    /// Spacing between the dates when a date range is requested, or null when the dates are event dates.
    /// </summary>
    public BalanceGranularityModel? Granularity { get; init; }
}

/// <summary>
/// A posted balance checkpoint for one event date.
/// </summary>
public sealed class BalanceEventDateBalanceModel
{
    /// <summary>
    /// Posting date.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Posted balance after the last matching event on this date.
    /// </summary>
    public required decimal TotalBalance { get; init; }
}
