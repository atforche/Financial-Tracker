namespace Models.FundGoals;

/// <summary>
/// Model comparing a Fund's financial state with its Fund Goal.
/// </summary>
public sealed class FundGoalProgressModel
{
    /// <summary>
    /// Gets contribution progress when configured.
    /// </summary>
    public ContributionProgressModel? Contribution { get; init; }

    /// <summary>
    /// Gets ending-balance progress, with a default minimum of zero.
    /// </summary>
    public required FundGoalEndingBalanceProgressModel EndingBalance { get; init; }
}
