namespace Data.Funds;

/// <summary>
/// Request to retrieve the Funds that match the specified criteria
/// </summary>
public record GetFundsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public FundSortOrder? SortBy { get; init; }

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