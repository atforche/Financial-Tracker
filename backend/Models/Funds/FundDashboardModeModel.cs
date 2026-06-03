namespace Models.Funds;

/// <summary>
/// Enum representing the time mode used to build the Fund dashboard response.
/// </summary>
public enum FundDashboardModeModel
{
    /// <summary>
    /// Dashboard data is grouped by Accounting Period.
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Dashboard data is grouped by date.
    /// </summary>
    Date,
}