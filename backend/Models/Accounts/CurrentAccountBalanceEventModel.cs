namespace Models.Accounts;

/// <summary>
/// Model representing a recent balance event for the current Accounts page.
/// </summary>
public class CurrentAccountBalanceEventModel
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
}