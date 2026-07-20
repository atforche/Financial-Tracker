namespace Models.FundPlans;

/// <summary>
/// Model comparing a Fund's financial state with its Fund Plan.
/// </summary>
public sealed class FundPlanProgressModel
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