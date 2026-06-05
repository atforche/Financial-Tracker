namespace Models.Transactions;

/// <summary>
/// Model representing summary counts and amounts for a specific Accounting Period.
/// </summary>
public class TransactionDashboardPeriodSummaryModel
{
    /// <summary>
    /// ID for the Accounting Period.
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Name for the Accounting Period.
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Year for the Accounting Period.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month for the Accounting Period.
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// Total count of transactions for this Accounting Period.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Total amount of transactions for this Accounting Period.
    /// </summary>
    public required decimal TotalAmount { get; init; }
}