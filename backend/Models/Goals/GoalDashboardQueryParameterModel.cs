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
    /// Optional Assignment Goal Type filters to apply to the dashboard.
    /// </summary>
    public IReadOnlyCollection<AssignmentGoalTypeModel>? AssignmentGoalType { get; init; }

    /// <summary>
    /// Optional Spending Goal Type filters to apply to the dashboard.
    /// </summary>
    public IReadOnlyCollection<SpendingGoalTypeModel>? SpendingGoalType { get; init; }

    /// <summary>
    /// Optional Fund Name filters to apply to the dashboard.
    /// </summary>
    public IReadOnlyCollection<string>? FundName { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Assignment Goals.
    /// </summary>
    public AssignmentGoalSortOrderModel? AssignmentSort { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Spending Goals.
    /// </summary>
    public SpendingGoalSortOrderModel? SpendingSort { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Assignment Goal balance events.
    /// </summary>
    public GoalDashboardBalanceEventSortOrderModel? AssignmentBalanceEventSort { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Spending Goal balance events.
    /// </summary>
    public GoalDashboardBalanceEventSortOrderModel? SpendingBalanceEventSort { get; init; }

    /// <summary>
    /// Maximum number of Assignment Goal results to return.
    /// </summary>
    public int? AssignmentGoalLimit { get; init; }

    /// <summary>
    /// Number of Assignment Goal results to skip.
    /// </summary>
    public int? AssignmentGoalOffset { get; init; }

    /// <summary>
    /// Maximum number of Spending Goal results to return.
    /// </summary>
    public int? SpendingGoalLimit { get; init; }

    /// <summary>
    /// Number of Spending Goal results to skip.
    /// </summary>
    public int? SpendingGoalOffset { get; init; }

    /// <summary>
    /// Maximum number of Assignment Goal balance events to return.
    /// </summary>
    public int? AssignmentBalanceEventLimit { get; init; }

    /// <summary>
    /// Number of Assignment Goal balance events to skip.
    /// </summary>
    public int? AssignmentBalanceEventOffset { get; init; }

    /// <summary>
    /// Maximum number of Spending Goal balance events to return.
    /// </summary>
    public int? SpendingBalanceEventLimit { get; init; }

    /// <summary>
    /// Number of Spending Goal balance events to skip.
    /// </summary>
    public int? SpendingBalanceEventOffset { get; init; }
}