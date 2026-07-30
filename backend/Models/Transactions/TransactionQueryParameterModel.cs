namespace Models.Transactions;

/// <summary>
/// Model representing the query parameters for getting Transactions
/// </summary>
public class TransactionQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filters to apply when retrieving the Transactions
    /// </summary>
    public TransactionFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public TransactionSortModel? Sort { get; init; }
}