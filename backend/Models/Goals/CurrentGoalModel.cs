namespace Models.Goals;

/// <summary>
/// Model representing the current goal snapshot for a single Fund.
/// </summary>
public class CurrentGoalModel
{
    /// <summary>
    /// Fund identifier.
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund name.
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Current assignment goal progress for the Fund.
    /// </summary>
    public required CurrentGoalProgressModel? AssignmentGoal { get; init; }

    /// <summary>
    /// Current spending goal progress for the Fund.
    /// </summary>
    public required CurrentGoalProgressModel? SpendingGoal { get; init; }
}