namespace Models.AccountingPeriods;

/// <summary>
/// Model representing a request to create a Budget Goal as part of creating an Accounting Period
/// </summary>
public class CreateAccountingPeriodBudgetGoalModel
{
    /// <summary>
    /// ID of the Budget to create a goal for
    /// </summary>
    public required Guid BudgetId { get; init; }

    /// <summary>
    /// Goal amount for this Budget in the new Accounting Period
    /// </summary>
    public required decimal GoalAmount { get; init; }
}
