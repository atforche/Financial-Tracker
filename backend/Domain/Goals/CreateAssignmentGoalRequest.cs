using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Record representing a request to create an <see cref="AssignmentGoal"/>.
/// </summary>
public record CreateAssignmentGoalRequest
{
    /// <summary>
    /// Fund for this Assignment Goal
    /// </summary>
    public Fund Fund { get; init; } = null!;

    /// <summary>
    /// Accounting Period for this Assignment Goal
    /// </summary>
    public AccountingPeriod? AccountingPeriod { get; init; }

    /// <summary>
    /// Type for this Assignment Goal
    /// </summary>
    public required AssignmentGoalType AssignmentGoalType { get; init; }

    /// <summary>
    /// Target amount for this Assignment Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}