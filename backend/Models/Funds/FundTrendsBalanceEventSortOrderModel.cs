namespace Models.Funds;

/// <summary>
/// Enum representing the different ways Fund trends balance events can be sorted.
/// </summary>
public enum FundTrendsBalanceEventSortOrderModel
{
    /// <summary>
    /// Balance events are sorted by fund name in ascending order.
    /// </summary>
    FundName,

    /// <summary>
    /// Balance events are sorted by fund name in descending order.
    /// </summary>
    FundNameDescending,

    /// <summary>
    /// Balance events are sorted by Accounting Period name in ascending order.
    /// </summary>
    AccountingPeriodName,

    /// <summary>
    /// Balance events are sorted by Accounting Period name in descending order.
    /// </summary>
    AccountingPeriodNameDescending,

    /// <summary>
    /// Balance events are sorted by effective date in ascending order.
    /// </summary>
    Date,

    /// <summary>
    /// Balance events are sorted by effective date in descending order.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Balance events are sorted by type in ascending order.
    /// </summary>
    Type,

    /// <summary>
    /// Balance events are sorted by type in descending order.
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Balance events are sorted by amount in ascending order.
    /// </summary>
    Amount,

    /// <summary>
    /// Balance events are sorted by amount in descending order.
    /// </summary>
    AmountDescending,
}