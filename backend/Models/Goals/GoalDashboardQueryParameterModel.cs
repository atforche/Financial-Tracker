namespace Models.Goals;

/// <summary>
/// Model representing the query parameters for the Goal dashboard endpoint.
/// </summary>
public class GoalDashboardQueryParameterModel
{
    /// <summary>
    /// ID for the first Accounting Period in the requested range.
    /// </summary>
    public Guid? StartAccountingPeriodId { get; init; }

    /// <summary>
    /// ID for the last Accounting Period in the requested range.
    /// </summary>
    public Guid? EndAccountingPeriodId { get; init; }

    /// <summary>
    /// Optional Goal Type filters to apply to the dashboard.
    /// </summary>
    public IReadOnlyCollection<GoalTypeModel>? GoalType { get; init; }

    /// <summary>
    /// Optional Fund Name filters to apply to the dashboard.
    /// </summary>
    public IReadOnlyCollection<string>? FundName { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Goals.
    /// </summary>
    public GoalSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip.
    /// </summary>
    public int? Offset { get; init; }
}
