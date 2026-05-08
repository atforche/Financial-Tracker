namespace Models.Goals;

/// <summary>
/// Model representing a request to update a Goal
/// </summary>
public class UpdateGoalModel
{
    /// <summary>
    /// Goal type for the Goal
    /// </summary>
    public required GoalTypeModel GoalType { get; init; }

    /// <summary>
    /// Goal amount for the Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}