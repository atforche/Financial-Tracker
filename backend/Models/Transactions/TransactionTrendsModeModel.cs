namespace Models.Transactions;

/// <summary>
/// Enum representing the time mode used to build the Transaction trends response.
/// </summary>
public enum TransactionTrendsModeModel
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