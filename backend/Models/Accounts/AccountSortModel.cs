namespace Models.Accounts;

/// <summary>
/// Enum representing the different ways Accounts can be sorted
/// </summary>
public enum AccountSortModel
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
}
