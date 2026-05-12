namespace Domain.Onboarding;

/// <summary>
/// Request to onboard a single Fund.
/// </summary>
public record OnboardingFundRequest
{
    /// <summary>
    /// Name for the Fund.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Fund.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Balance for the Fund.
    /// </summary>
    public required decimal Balance { get; init; }
}