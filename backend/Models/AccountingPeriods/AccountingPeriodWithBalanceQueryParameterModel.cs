namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for retrieving Accounting Periods with their balances.
/// </summary>
public class AccountingPeriodWithBalanceQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filter to apply to the results
    /// </summary>
    public AccountingPeriodFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AccountingPeriodWithBalanceSortModel? Sort { get; init; }
}