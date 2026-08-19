namespace Domain.Accounts.Queries;

/// <summary>
/// Supported Account Balance sort orders.
/// </summary>
public enum AccountBalanceSort
{
    /// <summary>
    /// Sorts Accounts by name in ascending order.
    /// </summary>
    Name,

    /// <summary>
    /// Sorts Accounts by name in descending order.
    /// </summary>
    NameDescending,

    /// <summary>
    /// Sorts Accounts by type in ascending order.
    /// </summary>
    Type,

    /// <summary>
    /// Sorts Accounts by type in descending order.
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Sorts Accounts by posted balance in ascending order.
    /// </summary>
    PostedBalance,

    /// <summary>
    /// Sorts Accounts by posted balance in descending order.
    /// </summary>
    PostedBalanceDescending,
}
