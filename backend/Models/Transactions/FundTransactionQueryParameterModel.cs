namespace Models.Transactions;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Transactions for a Fund 
/// </summary>
public class FundTransactionQueryParameterModel
{
    /// <summary>
    /// Accounting Period ID to filter the Transactions by
    /// </summary>
    public Guid? AccountingPeriodId { get; init; }

    /// <summary>
    /// Search to apply to the results
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public FundTransactionSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}