namespace Models.Funds;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Funds
/// </summary>
public class FundQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filter to apply to the results
    /// </summary>
    public FundFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public FundSortModel? Sort { get; init; }
}