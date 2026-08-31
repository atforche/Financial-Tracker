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
    /// Gets the new minimum ending balance.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Gets the new maximum ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}
