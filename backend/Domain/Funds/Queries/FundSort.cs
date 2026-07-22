namespace Domain.Funds.Queries;

/// <summary>
/// Supported Fund sort orders.
/// </summary>
public enum FundSort
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
}