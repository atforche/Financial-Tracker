namespace Data.Funds;

/// <summary>
/// Request to retrieve the Fund Goals that match the specified criteria
/// </summary>
public record GetFundGoalsRequest
{
    /// <summary>
    /// Search to apply to the results
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public FundGoalSortOrder? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}