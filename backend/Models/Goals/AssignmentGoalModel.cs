using Models.AccountingPeriods;
using Models.Funds;

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
    /// Fund for the Assignment Goal.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Accounting Period for the Assignment Goal, when one has been created.
    /// </summary>
    public required AccountingPeriodModel? AccountingPeriod { get; init; }

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
    /// Total amount assigned for the Assignment Goal
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Total amount assigned including pending assigned amounts for the Assignment Goal
    /// </summary>
    public required decimal TotalAmountAssignedIncludingPending { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Assignment Goal
    /// </summary>
    public required decimal RemainingAmountToAssign { get; init; }

    /// <summary>
    /// Remaining amount to assign including pending assigned amounts for the Assignment Goal
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