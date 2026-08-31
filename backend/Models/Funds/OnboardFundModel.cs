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
    /// Planned monthly contribution for the Fund Goal.
    /// </summary>
    public decimal? PlannedMonthlyContribution { get; init; }

    /// <summary>
    /// Minimum ending balance for the Fund Goal.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Maximum ending balance for the Fund Goal.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}
