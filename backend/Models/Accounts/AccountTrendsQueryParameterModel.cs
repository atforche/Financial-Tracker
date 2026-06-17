namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters for the Account trends endpoint.
/// </summary>
public class AccountTrendsQueryParameterModel
{
    /// <summary>
    /// First date in the requested range.
    /// </summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>
    /// Last date in the requested range.
    /// </summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// ID for the first Accounting Period in the requested range.
    /// </summary>
    public Guid? StartAccountingPeriodId { get; init; }

    /// <summary>
    /// ID for the last Accounting Period in the requested range.
    /// </summary>
    public Guid? EndAccountingPeriodId { get; init; }

    /// <summary>
    /// Optional Account Type filters to apply to the trends.
    /// </summary>
    public List<AccountTypeModel>? AccountType { get; init; }

    /// <summary>
    /// Optional Account Name filters to apply to the trends.
    /// </summary>
    public List<string>? AccountName { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Accounts.
    /// </summary>
    public AccountTrendsSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching balance events.
    /// </summary>
    public AccountTrendsBalanceEventSortOrderModel? BalanceEventSort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }

    /// <summary>
    /// Maximum number of balance events to return.
    /// </summary>
    public int? BalanceEventLimit { get; init; }

    /// <summary>
    /// Number of balance events to skip.
    /// </summary>
    public int? BalanceEventOffset { get; init; }
}