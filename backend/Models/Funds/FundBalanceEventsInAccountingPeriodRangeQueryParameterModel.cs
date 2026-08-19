namespace Models.Funds;

/// <summary>
/// Model representing the query parameters that can be applied when retrieving Fund balance events in an accounting period range.
/// </summary>
public class FundBalanceEventsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Accounting period range to apply to the results.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Filters to apply to the results.
    /// </summary>
    public FundFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort order to apply to the results.
    /// </summary>
    public FundBalanceEventSortModel? Sort { get; init; }
}
