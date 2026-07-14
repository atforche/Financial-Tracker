namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for retrieving Accounting Periods with their balances.
/// </summary>
public class AccountingPeriodWithBalanceQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Years to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Years { get; init; }

    /// <summary>
    /// Months to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Months { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AccountingPeriodWithBalanceSortModel? Sort { get; init; }
}