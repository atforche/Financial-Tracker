namespace Models.Accounts;

/// <summary>
/// Model representing an Account on the current Accounts page.
/// </summary>
public class CurrentAccountModel
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
    /// Current balance for the Account.
    /// </summary>
    public required AccountBalanceModel CurrentBalance { get; init; }

    /// <summary>
    /// Effective date for the most recent balance event affecting the Account.
    /// </summary>
    public required DateOnly? LastBalanceEventDate { get; init; }

    /// <summary>
    /// Most recent balance events affecting the Account.
    /// </summary>
    public required IReadOnlyCollection<CurrentAccountBalanceEventModel> RecentBalanceEvents { get; init; }
}