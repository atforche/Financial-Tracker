namespace Models.Accounts;

/// <summary>
/// Model representing a balance event in the Account workspace.
/// </summary>
public class AccountWorkspaceBalanceEventModel
{
    /// <summary>
    /// Transaction that produced this balance event.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Effective date of the balance event.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Type of balance event.
    /// </summary>
    public required AccountTrendsBalanceEventTypeModel Type { get; init; }

    /// <summary>
    /// Whether the transaction has been posted to the account.
    /// </summary>
    public required bool IsPosted { get; init; }

    /// <summary>
    /// Amount associated with the balance event.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Account balance before the transaction affected the account.
    /// </summary>
    public required decimal PreviousBalance { get; init; }

    /// <summary>
    /// Account balance after the transaction affected the account.
    /// </summary>
    public required decimal NewBalance { get; init; }
}