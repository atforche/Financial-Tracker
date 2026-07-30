namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters for getting Accounts within a specified date range.
/// </summary>
public class AccountsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Range of dates to get Accounts for.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Optional filters to apply to the results.
    /// </summary>
    public AccountFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional sort to apply to the results.
    /// </summary>
    public AccountWithBalanceRangeSortModel? Sort { get; init; }
}