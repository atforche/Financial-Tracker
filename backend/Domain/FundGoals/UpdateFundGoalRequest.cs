namespace Domain.FundGoals;

/// <summary>
/// Record representing a request to update a <see cref="FundGoal"/>.
/// </summary>
public sealed record UpdateFundGoalRequest
{
    /// <summary>
    /// Amount normally contributed during each Accounting Period.
    /// </summary>
    public decimal? PlannedMonthlyContribution { get; init; }

    /// <summary>
    /// Minimum desired balance at the end of an Accounting Period.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Maximum desired balance at the end of an Accounting Period.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}
