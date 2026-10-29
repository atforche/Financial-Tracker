namespace Models.Overview;

/// <summary>
/// Enum representing the length of the balance history shown on the overview
/// </summary>
public enum OverviewHistoryRangeModel
{
    /// <summary>
    /// The three months ending on the as-of date
    /// </summary>
    ThreeMonths,

    /// <summary>
    /// The year ending on the as-of date
    /// </summary>
    OneYear,

    /// <summary>
    /// The five years ending on the as-of date
    /// </summary>
    FiveYears,
}
