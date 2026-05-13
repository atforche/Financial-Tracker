namespace Domain.Accounts;

/// <summary>
/// Record representing a request to onboard an <see cref="Account"/>
/// </summary>
public record OnboardAccountRequest
{
    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for the Account
    /// </summary>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Onboarded Balance for the Account
    /// </summary>
    public required decimal OnboardedBalance { get; init; }
}