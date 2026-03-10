namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Accounting Periods
/// </summary>
public class AccountingPeriodQueryParameterModel
{
    /// <summary>
    /// Query string to apply to the results
    /// </summary>
    public string? QueryString { get; init; }

    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountingPeriodSortOrderModel? SortBy { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}