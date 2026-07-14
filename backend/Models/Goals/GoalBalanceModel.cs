namespace Models.Goals;

/// <summary>
/// Model representing the assignment and spending balance for a Goal.
/// </summary>
public class GoalBalanceModel
{
    /// <summary>
    /// Posted amount assigned to the Goal.
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Pending amount assigned to the Goal.
    /// </summary>
    public required decimal PendingAmountAssigned { get; init; }

    /// <summary>
    /// Posted amount spent from the Goal.
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Pending amount spent from the Goal.
    /// </summary>
    public required decimal PendingAmountSpent { get; init; }
}