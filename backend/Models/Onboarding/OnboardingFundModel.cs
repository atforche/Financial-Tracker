namespace Models.Onboarding;

/// <summary>
/// Model representing a Fund to create during onboarding.
/// </summary>
public class OnboardingFundModel
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