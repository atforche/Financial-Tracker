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
    /// Planned monthly contribution for the onboarded Fund Goal.
    /// </summary>
    public required decimal? PlannedMonthlyContribution { get; init; }

    /// <summary>
    /// Minimum ending balance for the onboarded Fund Goal.
    /// </summary>
    public required decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Maximum ending balance for the onboarded Fund Goal.
    /// </summary>
    public required decimal? MaximumEndingBalance { get; init; }
}
