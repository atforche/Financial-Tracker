namespace Models.Goals;

/// <summary>
/// Model representing Goal totals grouped by Accounting Period.
/// </summary>
public class GoalDashboardAccountingPeriodSummaryModel
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
    /// Total Goal amount for the group.
    /// </summary>
    public required decimal GoalAmount { get; init; }

    /// <summary>
    /// Total amount assigned for the group.
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Total amount spent for the group.
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Percentage of goals met for the group.
    /// </summary>
    public required decimal PercentageOfGoalsMet { get; init; }
}