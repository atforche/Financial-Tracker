namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters for getting Accounts within a specified accounting period range.
/// </summary>
public class AccountsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Range of accounting periods to get Accounts for.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Optional filters to apply to the results.
    /// </summary>
    public AccountFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional sort to apply to the results.
    /// </summary>
    public AccountWithBalanceRangeSortModel? Sort { get; init; }
}
