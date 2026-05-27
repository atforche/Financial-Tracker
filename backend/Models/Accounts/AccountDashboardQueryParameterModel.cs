namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters for the Account dashboard endpoint.
/// </summary>
public class AccountDashboardQueryParameterModel
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
    /// Optional Account Type filter to apply to the dashboard.
    /// </summary>
    public AccountTypeModel? AccountType { get; init; }

    /// <summary>
    /// Optional search string to apply to the matching Accounts.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Accounts.
    /// </summary>
    public AccountDashboardSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}