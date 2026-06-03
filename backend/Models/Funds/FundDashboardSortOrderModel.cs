namespace Models.Funds;

/// <summary>
/// Enum representing the different ways Fund dashboard rows can be sorted.
/// </summary>
public enum FundDashboardSortOrderModel
{
    /// <summary>
    /// Funds are sorted by name in ascending order.
    /// </summary>
    Name,

    /// <summary>
    /// Funds are sorted by name in descending order.
    /// </summary>
    NameDescending,

    /// <summary>
    /// Funds are sorted by opening balance in ascending order.
    /// </summary>
    OpeningBalance,

    /// <summary>
    /// Funds are sorted by opening balance in descending order.
    /// </summary>
    OpeningBalanceDescending,

    /// <summary>
    /// Funds are sorted by closing balance in ascending order.
    /// </summary>
    ClosingBalance,

    /// <summary>
    /// Funds are sorted by closing balance in descending order.
    /// </summary>
    ClosingBalanceDescending,

    /// <summary>
    /// Funds are sorted by net change (closing balance - opening balance) in ascending order.
    /// </summary>
    NetChange,

    /// <summary>
    /// Funds are sorted by net change (closing balance - opening balance) in descending order.
    /// </summary>
    NetChangeDescending,
}