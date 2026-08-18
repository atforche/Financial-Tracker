namespace Models.FundGoals;

/// <summary>
/// Model comparing a Fund's financial state with its Fund Goal.
/// </summary>
public sealed class FundGoalProgressModel
{
    /// <summary>
    /// Gets available-balance health.
    /// </summary>
    public required AvailableBalanceProgressModel AvailableBalance { get; init; }

    /// <summary>
    /// Gets contribution progress when configured.
    /// </summary>
    public ContributionProgressModel? Contribution { get; init; }

    /// <summary>
    /// Gets funded-balance progress when configured.
    /// </summary>
    public FundedBalanceProgressModel? FundedBalance { get; init; }

    /// <summary>
    /// Gets ending-balance progress when configured.
    /// </summary>
    public EndingBalanceProgressModel? EndingBalance { get; init; }
}
