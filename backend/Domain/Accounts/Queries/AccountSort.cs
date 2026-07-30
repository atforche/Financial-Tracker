namespace Domain.Accounts.Queries;

/// <summary>
/// Supported Account sort orders.
/// </summary>
public enum AccountSort
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
}