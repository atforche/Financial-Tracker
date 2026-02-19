namespace Data.AccountingPeriods;

/// <summary>
/// Request to retrieve the Accounting Periods that match the specified criteria
/// </summary>
public record GetAccountingPeriodsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountingPeriodSortOrder? SortBy { get; init; }

    /// <summary>
    /// Years to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Years { get; init; }

    /// <summary>
    /// Months to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Months { get; init; }

    /// <summary>
    /// True to include only open Accounting Periods in the results, false to include only closed Accounting Periods, null to include all Accounting Periods
    /// </summary>
    public bool? IsOpen { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}