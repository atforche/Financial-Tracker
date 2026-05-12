using Domain.Accounts;

namespace Domain.Onboarding;

/// <summary>
/// Request to onboard a single Account.
/// </summary>
public record OnboardingAccountRequest
{
    /// <summary>
    /// Name for the Account.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for the Account.
    /// </summary>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Balance for the Account.
    /// </summary>
    public required decimal Balance { get; init; }
}