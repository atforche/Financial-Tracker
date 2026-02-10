namespace Models.Accounts;

/// <summary>
/// Model representing an Account
/// </summary>
public class AccountModel
{
    /// <summary>
    /// ID for the Account
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for the Account
    /// </summary>
    public required AccountTypeModel Type { get; init; }

    /// <summary>
    /// Current Balance for the Account
    /// </summary>
    public required AccountBalanceModel CurrentBalance { get; init; }
}