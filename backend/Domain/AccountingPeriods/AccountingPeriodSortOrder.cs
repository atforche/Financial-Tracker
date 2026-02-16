namespace Domain.AccountingPeriods;

/// <summary>
/// Enum representing the different ways Accounting Periods can be sorted
/// </summary>
public enum AccountingPeriodSortOrder
{
    /// <summary>
    /// Accounting Periods are sorted by date in ascending order
    /// </summary>
    Date,

    /// <summary>
    /// Accounting Periods are sorted by date in descending order
    /// </summary>
    DateDescending,

    /// <summary>
    /// Accounting Periods are sorted by whether they are open in ascending order
    /// </summary>
    IsOpen,

    /// <summary>
    /// Accounting Periods are sorted by whether they are open in descending order
    /// </summary>
    IsOpenDescending,
}