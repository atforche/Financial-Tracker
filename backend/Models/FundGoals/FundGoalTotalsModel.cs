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
    /// Posted amount assigned toward the regular monthly contribution.
    /// </summary>
    public required decimal RegularAmountAssigned { get; init; }

    /// <summary>
    /// Amount assigned including unposted Transaction effects.
    /// </summary>
    public required decimal AmountAssignedIncludingPending { get; init; }

    /// <summary>
    /// Amount assigned toward the regular monthly contribution including
    /// unposted Transaction effects.
    /// </summary>
    public required decimal RegularAmountAssignedIncludingPending { get; init; }

    /// <summary>
    /// Posted amount spent.
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Amount spent including unposted Transaction effects.
    /// </summary>
    public required decimal AmountSpentIncludingPending { get; init; }
}
