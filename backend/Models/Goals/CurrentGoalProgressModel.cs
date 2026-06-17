namespace Models.Goals;

/// <summary>
/// Model representing a current goal progress section.
/// </summary>
public class CurrentGoalProgressModel
{
    /// <summary>
    /// Goal identifier.
    /// </summary>
    public required Guid GoalId { get; init; }

    /// <summary>
    /// Current target amount for this goal.
    /// </summary>
    public required decimal TargetAmount { get; init; }

    /// <summary>
    /// Current progress amount for this goal.
    /// </summary>
    public required decimal CurrentAmount { get; init; }

    /// <summary>
    /// Whether the goal is currently met.
    /// </summary>
    public required bool IsGoalMet { get; init; }

    /// <summary>
    /// Effective date for the most recent balance event affecting this goal.
    /// </summary>
    public required DateOnly? LastBalanceEventDate { get; init; }

    /// <summary>
    /// Most recent balance events affecting this goal.
    /// </summary>
    public required IReadOnlyCollection<CurrentGoalBalanceEventModel> RecentBalanceEvents { get; init; }
}