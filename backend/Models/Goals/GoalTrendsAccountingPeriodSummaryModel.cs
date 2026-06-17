namespace Models.Goals;

/// <summary>
/// Model representing Goal totals grouped by Accounting Period.
/// </summary>
public class GoalTrendsAccountingPeriodSummaryModel
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
    /// Total amount to assign for the group.
    /// </summary>
    public required decimal TotalAmountToAssign { get; init; }

    /// <summary>
    /// Total amount assigned for the group.
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Percentage of Assignment Goals met for the group.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfAssignmentGoalsMet { get; init; }

    /// <summary>
    /// Total amount to spend for the group.
    /// </summary>
    public required decimal TotalAmountToSpend { get; init; }

    /// <summary>
    /// Total amount spent for the group.
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }

    /// <summary>
    /// Percentage of Spending Goals met for the group.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfSpendingGoalsMet { get; init; }
}