namespace Models.Goals;

/// <summary>
/// Model representing a grouped Goal dashboard summary.
/// </summary>
public class GoalDashboardSummaryModel
{
    /// <summary>
    /// Name of the group (Goal Type or Accounting Period).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Total Goal amount for the group.
    /// </summary>
    public required decimal GoalAmount { get; init; }

    /// <summary>
    /// Total amount assigned for the group.
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Total amount spent for the group.
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Percentage of goals met for the group.
    /// </summary>
    public required decimal PercentageOfGoalsMet { get; init; }
}