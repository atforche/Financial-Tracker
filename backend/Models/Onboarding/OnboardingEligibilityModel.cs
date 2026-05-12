namespace Models.Onboarding;

/// <summary>
/// Model representing whether onboarding can be completed.
/// </summary>
public class OnboardingEligibilityModel
{
    /// <summary>
    /// Whether onboarding can be completed.
    /// </summary>
    public required bool IsEligible { get; init; }

    /// <summary>
    /// Errors preventing onboarding.
    /// </summary>
    public required IReadOnlyCollection<string> Errors { get; init; }
}