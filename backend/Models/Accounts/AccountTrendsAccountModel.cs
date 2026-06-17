namespace Models.Accounts;

/// <summary>
/// Model representing an Account row within the trends response.
/// </summary>
public class AccountTrendsAccountModel
{
    /// <summary>
    /// ID for the Account.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Account.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for the Account.
    /// </summary>
    public required AccountTypeModel Type { get; init; }

    /// <summary>
    /// Balance at the beginning of the requested range.
    /// </summary>
    public required decimal StartingBalance { get; init; }

    /// <summary>
    /// Balance at the end of the requested range.
    /// </summary>
    public required decimal EndingBalance { get; init; }
}