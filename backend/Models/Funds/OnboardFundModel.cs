namespace Models.Funds;

/// <summary>
/// Model representing a request to onboard a Fund.
/// </summary>
public class OnboardFundModel
{
    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Starting balance assigned during onboarding
    /// </summary>
    public required decimal OnboardedBalance { get; init; }

    /// <summary>
    /// Regular contribution for the Fund Goal.
    /// </summary>
    public decimal? RegularContribution { get; init; }

    /// <summary>
    /// Minimum funded balance for the Fund Goal.
    /// </summary>
    public decimal? MinimumFundedBalance { get; init; }

    /// <summary>
    /// Maximum funded balance for the Fund Goal.
    /// </summary>
    public decimal? MaximumFundedBalance { get; init; }

    /// <summary>
    /// Target ending balance for the Fund Goal.
    /// </summary>
    public decimal? TargetEndingBalance { get; init; }
}
