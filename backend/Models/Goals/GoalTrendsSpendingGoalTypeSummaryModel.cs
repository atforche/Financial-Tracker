namespace Models.Goals;

/// <summary>
/// Model representing spending goal totals grouped by spending goal type.
/// </summary>
public class GoalTrendsSpendingGoalTypeSummaryModel
{
    /// <summary>
    /// Spending Goal Type for the group.
    /// </summary>
    public required SpendingGoalTypeModel SpendingGoalType { get; init; }

    /// <summary>
    /// Total amount to spend for the group.
    /// </summary>
    public required decimal TotalAmountToSpend { get; init; }

    /// <summary>
    /// Total amount spent for the group.
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }

    /// <summary>
    /// Percentage of goals met for the group.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfGoalsMet { get; init; }
}