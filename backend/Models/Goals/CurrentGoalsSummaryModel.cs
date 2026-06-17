namespace Models.Goals;

/// <summary>
/// Model representing aggregate current-goal totals for the latest Accounting Period.
/// </summary>
public class CurrentGoalsSummaryModel
{
    /// <summary>
    /// Total amount to assign across current assignment goals.
    /// </summary>
    public required decimal TotalAmountToAssign { get; init; }

    /// <summary>
    /// Total amount assigned across current assignment goals.
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Percentage of current assignment goals met.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfAssignmentGoalsMet { get; init; }

    /// <summary>
    /// Total amount to spend across current spending goals.
    /// </summary>
    public required decimal TotalAmountToSpend { get; init; }

    /// <summary>
    /// Total amount spent across current spending goals.
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }

    /// <summary>
    /// Percentage of current spending goals met.
    /// </summary>
    public required GoalPercentageMetModel PercentageOfSpendingGoalsMet { get; init; }
}