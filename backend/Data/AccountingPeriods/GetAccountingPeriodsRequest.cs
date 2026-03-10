namespace Data.AccountingPeriods;

/// <summary>
/// Request to retrieve the Accounting Periods that match the specified criteria
/// </summary>
public record GetAccountingPeriodsRequest
{
    /// <summary>
    /// Query string to apply to the results
    /// </summary>
    public string? QueryString { get; init; }

    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountingPeriodSortOrder? SortBy { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}