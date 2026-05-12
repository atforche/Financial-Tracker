namespace Models.Onboarding;

/// <summary>
/// Model representing a request to complete onboarding.
/// </summary>
public class OnboardingModel
{
    /// <summary>
    /// Accounts to create during onboarding.
    /// </summary>
    public required IReadOnlyCollection<OnboardingAccountModel> Accounts { get; init; }

    /// <summary>
    /// Funds to create during onboarding.
    /// </summary>
    public required IReadOnlyCollection<OnboardingFundModel> Funds { get; init; }
}