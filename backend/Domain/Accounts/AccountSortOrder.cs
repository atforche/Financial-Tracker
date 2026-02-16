namespace Domain.Accounts;

/// <summary>
/// Enum representing the different ways Accounts can be sorted
/// </summary>
public enum AccountSortOrder
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
    /// Accounts are sorted by current balance in ascending order
    /// </summary>
    PostedBalance,

    /// <summary>
    /// Accounts are sorted by current balance in descending order
    /// </summary>
    PostedBalanceDescending,

    /// <summary>
    /// Accounts are sorted by available to spend in ascending order
    /// </summary>
    AvailableToSpend,

    /// <summary>
    /// Accounts are sorted by available to spend in descending order
    /// </summary>
    AvailableToSpendDescending,
}