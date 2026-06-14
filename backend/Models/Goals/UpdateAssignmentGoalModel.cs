namespace Models.Goals;

/// <summary>
/// Model representing a request to update an Assignment Goal
/// </summary>
public class UpdateAssignmentGoalModel
{
    /// <summary>
    /// Assignment goal type for the Goal
    /// </summary>
    public required AssignmentGoalTypeModel AssignmentGoalType { get; init; }

    /// <summary>
    /// Goal amount for the Assignment Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}