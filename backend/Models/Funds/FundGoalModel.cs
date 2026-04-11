namespace Models.Funds;

/// <summary>
/// Model representing a Fund Goal
/// </summary>
public class FundGoalModel
{
    /// <summary>
    /// ID for the Fund Goal
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Accounting Period ID for the Fund Goal
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Accounting Period name for the Fund Goal
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Goal amount for the Fund Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }

    /// <summary>
    /// Whether the goal has been met for the Fund Goal
    /// </summary>
    public required bool IsGoalMet { get; init; }
}