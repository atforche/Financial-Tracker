namespace Models.FundPlans;

/// <summary>
/// Model representing assignment and spending totals for a Fund Plan.
/// </summary>
public sealed class FundPlanTotalsModel
{
    /// <summary>
    /// Posted amount assigned.
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Pending amount assigned.
    /// </summary>
    public required decimal PendingAmountAssigned { get; init; }

    /// <summary>
    /// Posted amount spent.
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Pending amount spent.
    /// </summary>
    public required decimal PendingAmountSpent { get; init; }
}