namespace Models.Goals;

/// <summary>
/// Model representing assignment goal totals grouped by assignment goal type.
/// </summary>
public class GoalDashboardAssignmentGoalTypeSummaryModel
{
    /// <summary>
    /// Assignment Goal Type for the group.
    /// </summary>
    public required AssignmentGoalTypeModel AssignmentGoalType { get; init; }

    /// <summary>
    /// Total amount to assign for the group.
    /// </summary>
    public required decimal TotalAmountToAssign { get; init; }

    /// <summary>
    /// Total amount assigned for the group.
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Percentage of goals met for the group.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfGoalsMet { get; init; }
}