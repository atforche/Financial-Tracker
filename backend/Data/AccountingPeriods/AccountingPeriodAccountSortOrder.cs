namespace Data.AccountingPeriods;

/// <summary>
/// Enum representing the different ways Accounts within an Accounting Period can be sorted
/// </summary>
public enum AccountingPeriodAccountSortOrder
{
    /// <summary>
    /// Accounts are sorted by name in ascending order
    /// </summary>
    Name,

    /// <summary>
    /// Accounts are sorted by name in descending order
    /// </summary>
    NameDescending,

    /// <summary>
    /// Accounts are sorted by type in ascending order
    /// </summary>
    Type,

    /// <summary>
    /// Accounts are sorted by type in descending order
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Accounts are sorted by opening balance in ascending order
    /// </summary>
    OpeningBalance,

    /// <summary>
    /// Accounts are sorted by opening balance in descending order
    /// </summary>
    OpeningBalanceDescending,

    /// <summary>
    /// Accounts are sorted by closing balance in ascending order
    /// </summary>
    ClosingBalance,

    /// <summary>
    /// Accounts are sorted by closing balance in descending order
    /// </summary>
    ClosingBalanceDescending,
}