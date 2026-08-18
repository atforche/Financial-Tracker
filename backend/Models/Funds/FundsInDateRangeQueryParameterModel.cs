namespace Models.Funds;

/// <summary>
/// Model representing the query parameters for getting Funds within a specified date range.
/// </summary>
public class FundsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Range of dates to get Funds for.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Optional filters to apply to the results.
    /// </summary>
    public FundFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional sort to apply to the results.
    /// </summary>
    public FundWithBalanceRangeSortModel? Sort { get; init; }
}
