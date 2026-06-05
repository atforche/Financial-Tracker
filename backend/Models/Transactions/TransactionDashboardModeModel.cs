namespace Models.Transactions;

/// <summary>
/// Enum representing the time mode used to build the Transaction dashboard response.
/// </summary>
public enum TransactionDashboardModeModel
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