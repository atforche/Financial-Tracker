namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the different ways Funds within an Accounting Period can be sorted
/// </summary>
public enum AccountingPeriodFundSortOrderModel
{
    /// <summary>
    /// Funds are sorted by name in ascending order
    /// </summary>
    Name,

    /// <summary>
    /// Funds are sorted by name in descending order
    /// </summary>
    NameDescending,

    /// <summary>
    /// Funds are sorted by type in ascending order
    /// </summary>
    Type,

    /// <summary>
    /// Funds are sorted by type in descending order
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Funds are sorted by opening balance in ascending order
    /// </summary>
    OpeningBalance,

    /// <summary>
    /// Funds are sorted by opening balance in descending order
    /// </summary>
    OpeningBalanceDescending,

    /// <summary>
    /// Funds are sorted by amount assigned in ascending order
    /// </summary>
    AmountAssigned,

    /// <summary>
    /// Funds are sorted by amount assigned in descending order
    /// </summary>
    AmountAssignedDescending,

    /// <summary>
    /// Funds are sorted by amount spent in ascending order
    /// </summary>
    AmountSpent,

    /// <summary>
    /// Funds are sorted by amount spent in descending order
    /// </summary>
    AmountSpentDescending,

    /// <summary>
    /// Funds are sorted by closing balance in ascending order
    /// </summary>
    ClosingBalance,

    /// <summary>
    /// Funds are sorted by closing balance in descending order
    /// </summary>
    ClosingBalanceDescending,
}
