namespace Models.Funds;

/// <summary>
/// Enum representing the different ways Funds can be sorted.
/// </summary>
public enum FundSortModel
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
}