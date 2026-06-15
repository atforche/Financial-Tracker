namespace Models.Goals;

/// <summary>
/// Model representing the Goal dashboard response.
/// </summary>
public class GoalDashboardModel
{
    /// <summary>
    /// Matching Goals for the requested dashboard page.
    /// </summary>
    public required CollectionModel<AssignmentGoalModel> AssignmentGoals { get; init; }

    /// <summary>
    /// Matching balance events for Assignment Goals for the requested dashboard page.
    /// </summary>
    public required CollectionModel<GoalDashboardBalanceEventModel> AssignmentBalanceEvents { get; init; }

    /// <summary>
    /// Assignment Goal totals grouped by Goal Type.
    /// </summary>
    public IReadOnlyCollection<GoalDashboardAssignmentGoalTypeSummaryModel>? AssignmentGoalTypes { get; init; }

    /// <summary>
    /// Matching Spending Goals for the requested dashboard page.
    /// </summary>
    public required CollectionModel<SpendingGoalModel> SpendingGoals { get; init; }

    /// <summary>
    /// Matching balance events for Spending Goals for the requested dashboard page.
    /// </summary>
    public required CollectionModel<GoalDashboardBalanceEventModel> SpendingBalanceEvents { get; init; }

    /// <summary>
    /// Spending Goal totals grouped by SpendingGoal Type.
    /// </summary>
    public IReadOnlyCollection<GoalDashboardSpendingGoalTypeSummaryModel>? SpendingGoalTypes { get; init; }

    /// <summary>
    /// Available Fund names for the current dashboard scope before Fund-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Total amount to assign to assignment goals across the filtered results.
    /// </summary>
    public required decimal TotalAmountToAssign { get; init; }

    /// <summary>
    /// Total amount assigned across the filtered results.
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Percentage of Assignment Goals met across the filtered results.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfAssignmentGoalsMet { get; init; }

    /// <summary>
    /// Total amount to spend on spending goals across the filtered results.
    /// </summary>
    public required decimal TotalAmountToSpend { get; init; }

    /// <summary>
    /// Total amount spent on spending goals across the filtered results.
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }

    /// <summary>
    /// Percentage of Spending Goals met across the filtered results.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfSpendingGoalsMet { get; init; }

    /// <summary>
    /// Goal totals grouped by Accounting Period.
    /// </summary>
    public IReadOnlyCollection<GoalDashboardAccountingPeriodSummaryModel>? AccountingPeriods { get; init; }
}