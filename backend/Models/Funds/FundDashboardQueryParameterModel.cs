namespace Models.Funds;

/// <summary>
/// Model representing the query parameters for the Fund dashboard endpoint.
/// </summary>
public class FundDashboardQueryParameterModel
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
    /// Optional Fund Name filters to apply to the dashboard.
    /// </summary>
    public List<string>? FundName { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Funds.
    /// </summary>
    public FundDashboardSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching balance events.
    /// </summary>
    public FundDashboardBalanceEventSortOrderModel? BalanceEventSort { get; init; }

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