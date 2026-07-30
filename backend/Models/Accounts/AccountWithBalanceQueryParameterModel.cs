namespace Models.Accounts;

/// <summary>
/// Query parameters for retrieving Accounts with current balances.
/// </summary>
public class AccountWithBalanceQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filters to apply.
    /// </summary>
    public AccountFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply.
    /// </summary>
    public AccountWithBalanceSortModel? Sort { get; init; }
}