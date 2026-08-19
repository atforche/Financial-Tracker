namespace Domain.Funds;

/// <summary>
/// Record representing a request to onboard a <see cref="Fund"/>
/// </summary>
public record OnboardFundRequest
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
    /// Onboarded Balance for the Fund
    /// </summary>
    public required decimal OnboardedBalance { get; init; }

    /// <summary>
    /// Regular contribution for the onboarded Fund Goal.
    /// </summary>
    public required decimal? RegularContribution { get; init; }

    /// <summary>
    /// Minimum funded balance for the onboarded Fund Goal.
    /// </summary>
    public required decimal? MinimumFundedBalance { get; init; }

    /// <summary>
    /// Maximum funded balance for the onboarded Fund Goal.
    /// </summary>
    public required decimal? MaximumFundedBalance { get; init; }

    /// <summary>
    /// Target ending balance for the onboarded Fund Goal.
    /// </summary>
    public required decimal? TargetEndingBalance { get; init; }
}
