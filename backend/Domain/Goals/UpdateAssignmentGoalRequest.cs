namespace Domain.Goals;

/// <summary>
/// Request to update an <see cref="AssignmentGoal"/>.
/// </summary>
public record UpdateAssignmentGoalRequest
{
    /// <summary>
    /// Type for the Assignment Goal.
    /// </summary>
    public required AssignmentGoalType AssignmentGoalType { get; init; }

    /// <summary>
    /// Target amount for the Assignment Goal.
    /// </summary>
    public required decimal GoalAmount { get; init; }
}