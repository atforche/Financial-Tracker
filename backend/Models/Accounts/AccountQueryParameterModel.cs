namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Accounts
/// </summary>
public class AccountQueryParameterModel
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountSortOrderModel? SortBy { get; init; }

    /// <summary>
    /// Account names to include in the results
    /// </summary>
    public IReadOnlyCollection<string>? Names { get; init; }

    /// <summary>
    /// Account types to include in the results
    /// </summary>
    public IReadOnlyCollection<AccountTypeModel>? Types { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}