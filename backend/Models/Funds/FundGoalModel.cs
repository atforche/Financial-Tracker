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
    /// Fund ID for the Fund Goal
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund name for the Fund Goal
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Accounting Period ID for the Fund Goal
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Accounting Period name for the Fund Goal
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Goal type for the Fund Goal
    /// </summary>
    public required FundGoalTypeModel GoalType { get; init; }

    /// <summary>
    /// Goal amount for the Fund Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Fund Goal
    /// </summary>
    public required decimal RemainingAmountToAssign { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Fund Goal including pending assigned amounts
    /// </summary>
    public required decimal RemainingAmountToAssignIncludingPending { get; init; }

    /// <summary>
    /// Whether the assignment goal has been met for the Fund Goal
    /// </summary>
    public required bool IsAssignmentGoalMet { get; init; }

    /// <summary>
    /// Whether the assignment goal has been met for the Fund Goal including pending assigned amounts
    /// </summary>
    public required bool IsAssignmentGoalMetIncludingPending { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Fund Goal
    /// </summary>
    public required decimal RemainingAmountToSpend { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Fund Goal including pending spent amounts
    /// </summary>
    public required decimal RemainingAmountToSpendIncludingPending { get; init; }

    /// <summary>
    /// Whether the spending goal has been met for the Fund Goal
    /// </summary>
    public required bool IsSpendingGoalMet { get; init; }

    /// <summary>
    /// Whether the spending goal has been met for the Fund Goal including pending spent amounts
    /// </summary>
    public required bool IsSpendingGoalMetIncludingPending { get; init; }
}