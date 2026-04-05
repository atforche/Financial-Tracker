namespace Domain.Budgets;

/// <summary>
/// Request to create a Budget Goal as part of creating a new Accounting Period
/// </summary>
public record CreateAccountingPeriodBudgetGoalRequest
{
    /// <summary>
    /// The Budget to create a goal for
    /// </summary>
    public required Budget Budget { get; init; }

    /// <summary>
    /// Goal amount for this Budget in the new Accounting Period
    /// </summary>
    public required decimal GoalAmount { get; init; }
}
