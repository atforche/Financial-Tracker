namespace Models.FundPlans;

/// <summary>
/// Model describing contribution progress.
/// </summary>
public sealed class ContributionProgressModel
{
    /// <summary>
    /// Gets the recommended contribution.
    /// </summary>
    public required decimal TargetAmount { get; init; }

    /// <summary>
    /// Gets the amount assigned.
    /// </summary>
    public required decimal AssignedAmount { get; init; }

    /// <summary>
    /// Gets the nonnegative remaining amount.
    /// </summary>
    public required decimal RemainingAmount { get; init; }

    /// <summary>
    /// Gets whether the recommendation is satisfied.
    /// </summary>
    public required bool IsSatisfied { get; init; }
}