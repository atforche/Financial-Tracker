namespace Models.Funds;

/// <summary>
/// Enum representing the different ways Funds with balances can be sorted.
/// </summary>
public enum FundWithBalanceSortModel
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
    /// Funds are sorted by description in ascending order.
    /// </summary>
    Description,

    /// <summary>
    /// Funds are sorted by description in descending order.
    /// </summary>
    DescriptionDescending,

    /// <summary>
    /// Funds are sorted by current balance in ascending order.
    /// </summary>
    PostedBalance,

    /// <summary>
    /// Funds are sorted by current balance in descending order.
    /// </summary>
    PostedBalanceDescending,
}