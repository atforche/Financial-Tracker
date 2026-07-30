namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Supported Accounting Period Balance sort orders.
/// </summary>
public enum AccountingPeriodBalanceSort
{
    /// <summary>
    /// Sorts by date in ascending order.
    /// </summary>
    Date,

    /// <summary>
    /// Sorts by date in descending order.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Sorts by open status in ascending order.
    /// </summary>
    IsOpen,

    /// <summary>
    /// Sorts by open status in descending order.
    /// </summary>
    IsOpenDescending,

    /// <summary>
    /// Sorts by opening balance in ascending order.
    /// </summary>
    OpeningBalance,

    /// <summary>
    /// Sorts by opening balance in descending order.
    /// </summary>
    OpeningBalanceDescending,

    /// <summary>
    /// Sorts by closing balance in ascending order.
    /// </summary>
    ClosingBalance,

    /// <summary>
    /// Sorts by closing balance in descending order.
    /// </summary>
    ClosingBalanceDescending,
}