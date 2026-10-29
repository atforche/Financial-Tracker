namespace Domain.Overview;

/// <summary>
/// Length of the balance history shown on the overview, ending on the as-of date.
/// </summary>
public enum OverviewHistoryRange
{
    /// <summary>
    /// The three months ending on the as-of date.
    /// </summary>
    ThreeMonths,

    /// <summary>
    /// The year ending on the as-of date.
    /// </summary>
    OneYear,

    /// <summary>
    /// The five years ending on the as-of date.
    /// </summary>
    FiveYears,
}
