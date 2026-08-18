namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for retrieving Accounting Periods.
/// </summary>
public class AccountingPeriodQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filters to apply to the results
    /// </summary>
    public AccountingPeriodFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AccountingPeriodSortModel? Sort { get; init; }
}
