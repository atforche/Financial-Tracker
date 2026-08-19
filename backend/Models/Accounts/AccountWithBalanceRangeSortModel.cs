namespace Models.Accounts;

/// <summary>
/// Enum representing the different ways Accounts with balance ranges can be sorted.
/// </summary>
public enum AccountWithBalanceRangeSortModel
{
    /// <summary>
    /// Accounts are sorted by name in ascending order.
    /// </summary>
    Name,

    /// <summary>
    /// Accounts are sorted by name in descending order.
    /// </summary>
    NameDescending,

    /// <summary>
    /// Accounts are sorted by type in ascending order.
    /// </summary>
    Type,

    /// <summary>
    /// Accounts are sorted by type in descending order.
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Accounts are sorted by starting balance in ascending order.
    /// </summary>
    StartingBalance,

    /// <summary>
    /// Accounts are sorted by starting balance in descending order.
    /// </summary>
    StartingBalanceDescending,

    /// <summary>
    /// Accounts are sorted by ending balance in ascending order.
    /// </summary>
    EndingBalance,

    /// <summary>
    /// Accounts are sorted by ending balance in descending order.
    /// </summary>
    EndingBalanceDescending,

    /// <summary>
    /// Accounts are sorted by net change (ending balance - starting balance) in ascending order.
    /// </summary>
    NetChange,

    /// <summary>
    /// Accounts are sorted by net change (ending balance - starting balance) in descending order.
    /// </summary>
    NetChangeDescending,
}
