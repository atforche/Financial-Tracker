namespace Models.Funds;

/// <summary>
/// Model representing top-level trends balances for a specific Accounting Period.
/// </summary>
public class FundTrendsPeriodSummaryModel
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
    /// Total opening balance across all matching Funds.
    /// </summary>
    public required decimal TotalOpeningBalance { get; init; }

    /// <summary>
    /// Total closing balance across all matching Funds.
    /// </summary>
    public required decimal TotalClosingBalance { get; init; }

    /// <summary>
    /// Opening balance across assigned Funds.
    /// </summary>
    public required decimal AssignedOpeningBalance { get; init; }
    /// <summary> 
    /// Closing balance across assigned Funds.
    /// </summary>
    public required decimal AssignedClosingBalance { get; init; }

    /// <summary>
    /// Opening balance across unassigned Funds.
    /// </summary>
    public required decimal UnassignedOpeningBalance { get; init; }

    /// <summary> 
    /// Closing balance across unassigned Funds.
    /// </summary>
    public required decimal UnassignedClosingBalance { get; init; }
}