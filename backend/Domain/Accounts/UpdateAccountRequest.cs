namespace Domain.Accounts;

/// <summary>
/// Record representing a request to update an <see cref="Account"/>
/// </summary>
public record UpdateAccountRequest
{
    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Financial institution for the Account.
    /// </summary>
    public string? FinancialInstitution { get; init; }
}