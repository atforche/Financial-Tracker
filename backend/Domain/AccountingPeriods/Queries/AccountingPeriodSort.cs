namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Supported Accounting Period sort orders.
/// </summary>
public enum AccountingPeriodSort
{
    /// <summary>
    /// Sorts Accounting Periods by date in ascending order.
    /// </summary>
    Date,

    /// <summary>
    /// Sorts Accounting Periods by date in descending order.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Sorts Accounting Periods by open status in ascending order.
    /// </summary>
    IsOpen,

    /// <summary>
    /// Sorts Accounting Periods by open status in descending order.
    /// </summary>
    IsOpenDescending,
}