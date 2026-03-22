namespace Data.AccountingPeriods;

/// <summary>
/// Request to retrieve the Accounts within an Accounting Period that match the specified criteria
/// </summary>
public record GetAccountingPeriodAccountsRequest
{
    /// <summary>
    /// Search to apply to the results
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AccountingPeriodAccountSortOrder? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}