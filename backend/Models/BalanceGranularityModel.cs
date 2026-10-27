namespace Models;

/// <summary>
/// Enum representing the spacing between the points in a balance series
/// </summary>
public enum BalanceGranularityModel
{
    /// <summary>
    /// One point at the end of each day
    /// </summary>
    Day,

    /// <summary>
    /// One point at the end of each week, with weeks ending on Saturday
    /// </summary>
    Week,

    /// <summary>
    /// One point at the end of each calendar month
    /// </summary>
    Month,
}
