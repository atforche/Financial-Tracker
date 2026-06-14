namespace Models.Goals;

/// <summary>
/// Model representing an Assignment Goal
/// </summary>
public class AssignmentGoalModel
{
    /// <summary>
    /// ID for the Assignment Goal
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Fund ID for the Assignment Goal
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund name for the Assignment Goal
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Accounting Period ID for the Assignment Goal
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Accounting Period name for the Assignment Goal
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Type for the Assignment Goal
    /// </summary>
    public required AssignmentGoalTypeModel Type { get; init; }

    /// <summary>
    /// Goal amount for the Assignment Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }

    /// <summary>
    /// Total amount to assign for the Assignment Goal
    /// </summary>
    public required decimal TotalAmountToAssign { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Assignment Goal
    /// </summary>
    public required decimal RemainingAmountToAssign { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Assignment Goal including pending assigned amounts
    /// </summary>
    public required decimal RemainingAmountToAssignIncludingPending { get; init; }

    /// <summary>
    /// Whether the Assignment Goal has been met
    /// </summary>
    public required bool IsGoalMet { get; init; }

    /// <summary>
    /// Whether the Assignment Goal has been met including pending assigned amounts
    /// </summary>
    public required bool IsGoalMetIncludingPending { get; init; }
}