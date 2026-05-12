using Models.Accounts;

namespace Models.Onboarding;

/// <summary>
/// Model representing an Account to create during onboarding.
/// </summary>
public class OnboardingAccountModel
{
    /// <summary>
    /// Name for the Account.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for the Account.
    /// </summary>
    public required AccountTypeModel Type { get; init; }

    /// <summary>
    /// Balance for the Account.
    /// </summary>
    public required decimal Balance { get; init; }
}