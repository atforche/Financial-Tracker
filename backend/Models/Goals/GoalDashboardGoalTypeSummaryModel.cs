namespace Models.Goals;

/// <summary>
/// Model representing Goal totals grouped by Goal Type.
/// </summary>
public class GoalDashboardGoalTypeSummaryModel
{
    /// <summary>
    /// Goal Type for the group.
    /// </summary>
    public required GoalTypeModel GoalType { get; init; }

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
