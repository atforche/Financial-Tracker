namespace Models.Accounts;

/// <summary>
/// Model representing a balance event on the Account dashboard.
/// </summary>
public class AccountDashboardBalanceEventModel
{
    /// <summary>
    /// Account affected by the balance event.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Name of the account affected by the balance event.
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Effective date of the balance event.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Accounting Period containing the balance event.
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Name of the Accounting Period containing the balance event.
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Type of balance event.
    /// </summary>
    public required AccountDashboardBalanceEventTypeModel Type { get; init; }

    /// <summary>
    /// Whether the transaction has been posted to the account.
    /// </summary>
    public required bool IsPosted { get; init; }

    /// <summary>
    /// Amount associated with the balance event.
    /// </summary>
    public required decimal Amount { get; init; }
}