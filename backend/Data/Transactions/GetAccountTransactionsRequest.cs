namespace Data.Transactions;

/// <summary>
/// Request to retrieve the Transactions within an Account that match the specified criteria
/// </summary>
public record GetAccountTransactionsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountTransactionSortOrder? SortBy { get; init; }

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
    public IReadOnlyCollection<TransactionType>? Types { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}