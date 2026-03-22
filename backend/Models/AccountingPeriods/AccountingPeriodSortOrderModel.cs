namespace Models.AccountingPeriods;

/// <summary>
/// Enum representing the different ways Accounting Periods can be sorted
/// </summary>
public enum AccountingPeriodSortOrderModel
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

    /// <summary>
    /// Accounting Periods are sorted by opening balance in ascending order
    /// </summary>
    OpeningBalance,

    /// <summary>
    /// Accounting Periods are sorted by opening balance in descending order
    /// </summary>
    OpeningBalanceDescending,

    /// <summary>
    /// Accounting Periods are sorted by closing balance in ascending order
    /// </summary>
    ClosingBalance,

    /// <summary>
    /// Accounting Periods are sorted by closing balance in descending order
    /// </summary>
    ClosingBalanceDescending
}