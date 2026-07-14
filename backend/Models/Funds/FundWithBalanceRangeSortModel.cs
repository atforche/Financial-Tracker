namespace Models.Funds;

/// <summary>
/// Enum representing the different ways Funds with balance ranges can be sorted.
/// </summary>
public enum FundWithBalanceRangeSortModel
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
    /// Funds are sorted by starting balance in ascending order.
    /// </summary>
    StartingBalance,

    /// <summary>
    /// Funds are sorted by starting balance in descending order.
    /// </summary>
    StartingBalanceDescending,

    /// <summary>
    /// Funds are sorted by ending balance in ascending order.
    /// </summary>
    EndingBalance,

    /// <summary>
    /// Funds are sorted by ending balance in descending order.
    /// </summary>
    EndingBalanceDescending,

    /// <summary>
    /// Funds are sorted by net change (closing balance - opening balance) in ascending order.
    /// </summary>
    NetChange,

    /// <summary>
    /// Funds are sorted by net change (closing balance - opening balance) in descending order.
    /// </summary>
    NetChangeDescending,
}