using Domain.AccountingPeriods;

namespace Domain.Budgets;

/// <summary>
/// Record representing a request to create a <see cref="BudgetGoal"/>
/// </summary>
public record CreateBudgetGoalRequest
{
    /// <summary>
    /// Budget for this Budget Goal
    /// </summary>
    public Budget Budget { get; init; } = null!;

    /// <summary>
    /// Accounting Period for this Budget Goal
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; } = null!;

    /// <summary>
    /// Target goal amount for the Budget Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}
