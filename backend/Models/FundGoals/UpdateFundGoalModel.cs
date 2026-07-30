namespace Models.FundGoals;

/// <summary>
/// Model representing Fund Goal configuration updates.
/// </summary>
public sealed class UpdateFundGoalModel
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