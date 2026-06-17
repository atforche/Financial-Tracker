namespace Models.Funds;

/// <summary>
/// Enum representing the time mode used to build the Fund trends response.
/// </summary>
public enum FundTrendsModeModel
{
    /// <summary>
    /// Trends data is grouped by Accounting Period.
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Trends data is grouped by date.
    /// </summary>
    Date,
}