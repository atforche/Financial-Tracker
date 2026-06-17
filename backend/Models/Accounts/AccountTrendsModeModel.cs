namespace Models.Accounts;

/// <summary>
/// Enum representing the time mode used to build the Account trends response.
/// </summary>
public enum AccountTrendsModeModel
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