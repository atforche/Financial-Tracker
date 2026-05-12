namespace Domain.Onboarding;

/// <summary>
/// Request to onboard the initial set of Accounts and Funds.
/// </summary>
public record OnboardingRequest
{
    /// <summary>
    /// Accounts to create during onboarding.
    /// </summary>
    public required IReadOnlyCollection<OnboardingAccountRequest> Accounts { get; init; }

    /// <summary>
    /// Funds to create during onboarding.
    /// </summary>
    public required IReadOnlyCollection<OnboardingFundRequest> Funds { get; init; }
}