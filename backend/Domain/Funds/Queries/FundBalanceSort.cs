namespace Domain.Funds.Queries;

/// <summary>
/// Supported Fund Balance sort orders.
/// </summary>
public enum FundBalanceSort
{
    /// <summary>
    /// Sorts Funds by name in ascending order.
    /// </summary>
    Name,

    /// <summary>
    /// Sorts Funds by name in descending order.
    /// </summary>
    NameDescending,

    /// <summary>
    /// Sorts Funds by description in ascending order.
    /// </summary>
    Description,

    /// <summary>
    /// Sorts Funds by description in descending order.
    /// </summary>
    DescriptionDescending,

    /// <summary>
    /// Sorts Funds by posted balance in ascending order.
    /// </summary>
    PostedBalance,

    /// <summary>
    /// Sorts Funds by posted balance in descending order.
    /// </summary>
    PostedBalanceDescending,
}
