namespace Models.Funds;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Funds
/// </summary>
public class FundQueryParameterModel
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public FundSortOrderModel? SortBy { get; init; }

    /// <summary>
    /// Fund names to include in the results
    /// </summary>
    public IReadOnlyCollection<string>? Names { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}