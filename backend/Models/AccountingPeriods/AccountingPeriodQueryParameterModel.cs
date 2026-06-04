namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Accounting Periods
/// </summary>
public class AccountingPeriodQueryParameterModel
{
    /// <summary>
    /// Years to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Years { get; init; }

    /// <summary>
    /// Months to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Months { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AccountingPeriodSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}