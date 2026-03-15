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
    /// Funds are sorted by description in ascending order
    /// </summary>
    Description,

    /// <summary>
    /// Funds are sorted by description in descending order
    /// </summary>
    DescriptionDescending,

    /// <summary>
    /// Funds are sorted by opening balance in ascending order
    /// </summary>
    OpeningBalance,

    /// <summary>
    /// Funds are sorted by opening balance in descending order
    /// </summary>
    OpeningBalanceDescending,

    /// <summary>
    /// Funds are sorted by closing balance in ascending order
    /// </summary>
    ClosingBalance,

    /// <summary>
    /// Funds are sorted by closing balance in descending order
    /// </summary>
    ClosingBalanceDescending,
}