namespace Models.Accounts;

/// <summary>
/// Model representing a request to onboard an Account.
/// </summary>
public class OnboardAccountModel
{
    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of the Account
    /// </summary>
    public required AccountTypeModel Type { get; init; }

    /// <summary>
    /// Starting balance assigned during onboarding
    /// </summary>
    public required decimal OnboardedBalance { get; init; }
}