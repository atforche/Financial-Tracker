using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Record representing a request to create a <see cref="SpendingGoal"/>.
/// </summary>
public record CreateSpendingGoalRequest
{
    /// <summary>
    /// Fund for this Spending Goal
    /// </summary>
    public Fund Fund { get; init; } = null!;

    /// <summary>
    /// Accounting Period for this Spending Goal
    /// </summary>
    public AccountingPeriod? AccountingPeriod { get; init; }

    /// <summary>
    /// Type for this Spending Goal
    /// </summary>
    public required SpendingGoalType SpendingGoalType { get; init; }
}