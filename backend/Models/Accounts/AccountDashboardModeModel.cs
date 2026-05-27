namespace Models.Accounts;

/// <summary>
/// Enum representing the time mode used to build the Account dashboard response.
/// </summary>
public enum AccountDashboardModeModel
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