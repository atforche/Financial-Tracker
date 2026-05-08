using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Record representing a request to create a <see cref="Goal"/>
/// </summary>
public record CreateGoalRequest
{
    /// <summary>
    /// Fund for this Goal
    /// </summary>
    public Fund Fund { get; init; } = null!;

    /// <summary>
    /// Accounting Period for this Goal
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; } = null!;

    /// <summary>
    /// Type for this Goal
    /// </summary>
    public required GoalType GoalType { get; init; }

    /// <summary>
    /// Target goal amount for this Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}