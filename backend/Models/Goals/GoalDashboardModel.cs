namespace Models.Goals;

/// <summary>
/// Model representing the Goal dashboard response.
/// </summary>
public class GoalDashboardModel
{
    /// <summary>
    /// Matching Goals for the requested dashboard page.
    /// </summary>
    public required CollectionModel<GoalModel> Goals { get; init; }

    /// <summary>
    /// Available Fund names for the current dashboard scope before Fund-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Total Goal amount across the filtered results.
    /// </summary>
    public required decimal TotalGoalAmount { get; init; }

    /// <summary>
    /// Total amount assigned across the filtered results.
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Total amount spent across the filtered results.
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }

    /// <summary>
    /// Percentage of goals met across the filtered results.
    /// </summary>
    public required decimal PercentageOfGoalsMet { get; init; }

    /// <summary>
    /// Goal totals grouped by Goal Type.
    /// </summary>
    public IReadOnlyCollection<GoalDashboardGoalTypeSummaryModel>? GoalTypes { get; init; }

    /// <summary>
    /// Goal totals grouped by Accounting Period.
    /// </summary>
    public IReadOnlyCollection<GoalDashboardAccountingPeriodSummaryModel>? AccountingPeriods { get; init; }
}
