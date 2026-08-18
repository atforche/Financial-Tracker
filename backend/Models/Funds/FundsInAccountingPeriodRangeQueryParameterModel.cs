namespace Models.Funds;

/// <summary>
/// Model representing the query parameters for getting Funds within a specified accounting period range.
/// </summary>
public class FundsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Range of accounting periods to get Funds for.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Optional filters to apply to the results.
    /// </summary>
    public FundFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional sort to apply to the results.
    /// </summary>
    public FundWithBalanceRangeSortModel? Sort { get; init; }
}
