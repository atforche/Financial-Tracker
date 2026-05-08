namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Transactions for an Accounting Period
/// </summary>
public class AccountingPeriodTransactionQueryParameterModel
{
    /// <summary>
    /// Search string to apply to the results
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AccountingPeriodTransactionSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}