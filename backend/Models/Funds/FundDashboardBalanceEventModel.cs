namespace Models.Funds;

/// <summary>
/// Model representing a balance event on the Fund dashboard.
/// </summary>
public class FundDashboardBalanceEventModel
{
    /// <summary>
    /// Fund affected by the balance event.
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Name of the fund affected by the balance event.
    /// </summary>
    public required string FundName { get; init; }

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
    public required FundDashboardBalanceEventTypeModel Type { get; init; }

    /// <summary>
    /// Whether the transaction has been posted to the fund.
    /// </summary>
    public required bool IsPosted { get; init; }

    /// <summary>
    /// Amount associated with the balance event.
    /// </summary>
    public required decimal Amount { get; init; }
}