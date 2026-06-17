namespace Models.Transactions;

/// <summary>
/// Model representing the query parameters for the Transaction trends endpoint.
/// </summary>
public class TransactionTrendsQueryParameterModel
{
    /// <summary>
    /// First date in the requested range.
    /// </summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>
    /// Last date in the requested range.
    /// </summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// ID for the first Accounting Period in the requested range.
    /// </summary>
    public Guid? StartAccountingPeriodId { get; init; }

    /// <summary>
    /// ID for the last Accounting Period in the requested range.
    /// </summary>
    public Guid? EndAccountingPeriodId { get; init; }

    /// <summary>
    /// Optional Transaction Type filters to apply to the trends.
    /// </summary>
    public List<TransactionTypeModel>? TransactionType { get; init; }

    /// <summary>
    /// Optional Account Name filters to apply to the trends.
    /// </summary>
    public List<string>? AccountName { get; init; }

    /// <summary>
    /// Optional Fund Name filters to apply to the trends.
    /// </summary>
    public List<string>? FundName { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Transactions.
    /// </summary>
    public TransactionSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip.
    /// </summary>
    public int? Offset { get; init; }
}