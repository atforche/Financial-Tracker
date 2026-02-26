using Domain.Accounts;

namespace Data.Accounts;

/// <summary>
/// Request to retrieve the Accounts that match the specified criteria
/// </summary>
public record GetAccountsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountSortOrder? SortBy { get; init; }

    /// <summary>
    /// Account names to include in the results
    /// </summary>
    public IReadOnlyCollection<string>? Names { get; init; }

    /// <summary>
    /// Account types to include in the results
    /// </summary>
    public IReadOnlyCollection<AccountType>? Types { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}