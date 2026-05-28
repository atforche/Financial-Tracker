namespace Models.Accounts;

/// <summary>
/// Enum representing the different ways Account dashboard rows can be sorted.
/// </summary>
public enum AccountDashboardSortOrderModel
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
    /// Accounts are sorted by opening balance in ascending order.
    /// </summary>
    OpeningBalance,

    /// <summary>
    /// Accounts are sorted by opening balance in descending order.
    /// </summary>
    OpeningBalanceDescending,

    /// <summary>
    /// Accounts are sorted by closing balance in ascending order.
    /// </summary>
    ClosingBalance,

    /// <summary>
    /// Accounts are sorted by closing balance in descending order.
    /// </summary>
    ClosingBalanceDescending,

    /// <summary>
    /// Accounts are sorted by net change (closing balance - opening balance) in ascending order.
    /// </summary>
    NetChange,

    /// <summary>
    /// Accounts are sorted by net change (closing balance - opening balance) in descending order.
    /// </summary>
    NetChangeDescending,
}