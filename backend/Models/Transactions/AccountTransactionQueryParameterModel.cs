namespace Models.Transactions;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Transactions for an Account
/// </summary>
public class AccountTransactionQueryParameterModel
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountTransactionSortOrderModel? SortBy { get; init; }

    /// <summary>
    /// Minimum date to include in the results
    /// </summary>
    public DateOnly? MinDate { get; init; }

    /// <summary>
    /// Maximum date to include in the results
    /// </summary>
    public DateOnly? MaxDate { get; init; }

    /// <summary>
    /// Locations to include in the results
    /// </summary>
    public IReadOnlyCollection<string>? Locations { get; init; }

    /// <summary>
    /// Types to include in the results
    /// </summary>
    public IReadOnlyCollection<TransactionTypeModel>? Types { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}