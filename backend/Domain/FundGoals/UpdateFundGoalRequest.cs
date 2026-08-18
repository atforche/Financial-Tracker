namespace Domain.FundGoals;

/// <summary>
/// Record representing a request to update a <see cref="FundGoal"/>.
/// </summary>
public sealed record UpdateFundGoalRequest
{
    /// <summary>
    /// Amount normally contributed during each Accounting Period.
    /// </summary>
    public decimal? RegularContribution { get; init; }

    /// <summary>
    /// Minimum desired balance immediately after assignments.
    /// </summary>
    public decimal? MinimumFundedBalance { get; init; }

    /// <summary>
    /// Maximum desired balance immediately after assignments.
    /// </summary>
    public decimal? MaximumFundedBalance { get; init; }

    /// <summary>
    /// Desired balance at the end of an Accounting Period.
    /// </summary>
    public decimal? TargetEndingBalance { get; init; }
}
