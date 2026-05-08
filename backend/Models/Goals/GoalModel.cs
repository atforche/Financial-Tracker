namespace Models.Goals;

/// <summary>
/// Model representing a Goal
/// </summary>
public class GoalModel
{
    /// <summary>
    /// ID for the Goal
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Fund ID for the Goal
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund name for the Goal
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Accounting Period ID for the Goal
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Accounting Period name for the Goal
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Goal type for the Goal
    /// </summary>
    public required GoalTypeModel GoalType { get; init; }

    /// <summary>
    /// Goal amount for the Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Goal
    /// </summary>
    public required decimal RemainingAmountToAssign { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Goal including pending assigned amounts
    /// </summary>
    public required decimal RemainingAmountToAssignIncludingPending { get; init; }

    /// <summary>
    /// Whether the assignment goal has been met for the Goal
    /// </summary>
    public required bool IsAssignmentGoalMet { get; init; }

    /// <summary>
    /// Whether the assignment goal has been met for the Goal including pending assigned amounts
    /// </summary>
    public required bool IsAssignmentGoalMetIncludingPending { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Goal
    /// </summary>
    public required decimal RemainingAmountToSpend { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Goal including pending spent amounts
    /// </summary>
    public required decimal RemainingAmountToSpendIncludingPending { get; init; }

    /// <summary>
    /// Whether the spending goal has been met for the Goal
    /// </summary>
    public required bool IsSpendingGoalMet { get; init; }

    /// <summary>
    /// Whether the spending goal has been met for the Goal including pending spent amounts
    /// </summary>
    public required bool IsSpendingGoalMetIncludingPending { get; init; }
}