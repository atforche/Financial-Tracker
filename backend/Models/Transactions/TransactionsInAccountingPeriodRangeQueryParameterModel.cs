namespace Models.Transactions;

/// <summary>
/// Model representing the query parameters for getting Transactions within a specified accounting period range.
/// </summary>
public class TransactionsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Accounting Period Range to filter the Transactions by
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Filters to apply when retrieving the Transactions
    /// </summary>
    public TransactionFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public TransactionSortModel? Sort { get; init; }
}