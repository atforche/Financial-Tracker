namespace Models.Transactions;

/// <summary>
/// Model representing the query parameters for getting Transactions within a specified date range.
/// </summary>
public class TransactionsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Date Range to filter the Transactions by
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Filters to apply when retrieving the Transactions
    /// </summary>
    public TransactionFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public TransactionSortModel? Sort { get; init; }
}
