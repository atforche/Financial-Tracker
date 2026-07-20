using Domain.Funds;

namespace Domain.FundPlans;

/// <summary>
/// Record representing a request to create a <see cref="FundPlan"/>.
/// </summary>
public sealed record CreateFundPlanRequest
{
    /// <summary>
    /// Fund associated with the Fund Plan.
    /// </summary>
    public Fund Fund { get; init; } = null!;

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