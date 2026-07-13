namespace Domain.Goals;

/// <summary>
/// Request to update a <see cref="SpendingGoal"/>.
/// </summary>
public record UpdateSpendingGoalRequest
{
    /// <summary>
    /// Type for the Spending Goal.
    /// </summary>
    public required SpendingGoalType SpendingGoalType { get; init; }
}
