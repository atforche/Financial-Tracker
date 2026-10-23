namespace Models.FundGoals;

/// <summary>
/// Model representing assignment and spending totals for a Fund Goal.
/// </summary>
public sealed class FundGoalTotalsModel
{
    /// <summary>
    /// Posted amount assigned.
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Posted amount assigned toward the expected monthly contribution.
    /// </summary>
    public required decimal AmountAssignedToExpectedContribution { get; init; }

    /// <summary>
    /// Amount remaining to assign toward the expected monthly contribution.
    /// </summary>
    public required decimal RemainingAmountToAssignToExpectedContribution { get; init; }

    /// <summary>
    /// Posted amount spent.
    /// </summary>
    public required decimal AmountSpent { get; init; }
}
