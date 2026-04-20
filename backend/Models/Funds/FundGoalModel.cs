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
    /// Goal type for the Fund Goal
    /// </summary>
    public required FundGoalTypeModel GoalType { get; init; }

    /// <summary>
    /// Opening balance for the Fund Goal
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// Goal amount for the Fund Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }

    /// <summary>
    /// Amount assigned for the Fund Goal
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Pending amount assigned for the Fund Goal
    /// </summary>
    public required decimal PendingAmountAssigned { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Fund Goal
    /// </summary>
    public required decimal RemainingAmountToAssign { get; init; }

    /// <summary>
    /// Whether the assignment goal has been met for the Fund Goal
    /// </summary>
    public required bool IsAssignmentGoalMet { get; init; }

    /// <summary>
    /// Amount spent for the Fund Goal
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Pending amount spent for the Fund Goal
    /// </summary>
    public required decimal PendingAmountSpent { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Fund Goal
    /// </summary>
    public required decimal RemainingAmountToSpend { get; init; }

    /// <summary>
    /// Whether the spending goal has been met for the Fund Goal
    /// </summary>
    public required bool IsSpendingGoalMet { get; init; }

    /// <summary>
    /// Closing balance for the Fund Goal
    /// </summary>
    public required decimal ClosingBalance { get; init; }
}