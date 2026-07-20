namespace Models.FundPlans;

/// <summary>
/// Model representing Fund Plan configuration updates.
/// </summary>
public sealed class UpdateFundPlanModel
{
    /// <summary>
    /// Gets the new regular contribution.
    /// </summary>
    public decimal? RegularContribution { get; init; }

    /// <summary>
    /// Gets the new minimum funded balance.
    /// </summary>
    public decimal? MinimumFundedBalance { get; init; }

    /// <summary>
    /// Gets the new maximum funded balance.
    /// </summary>
    public decimal? MaximumFundedBalance { get; init; }

    /// <summary>
    /// Gets the new target ending balance.
    /// </summary>
    public decimal? TargetEndingBalance { get; init; }
}