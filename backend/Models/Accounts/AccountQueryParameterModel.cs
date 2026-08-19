namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Accounts
/// </summary>
public class AccountQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filter to apply to the results
    /// </summary>
    public AccountFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AccountSortModel? Sort { get; init; }
}
