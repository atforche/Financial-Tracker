namespace Models.Goals;

/// <summary>
/// Model representing a request to update a Spending Goal
/// </summary>
public class UpdateSpendingGoalModel
{
    /// <summary>
    /// Spending goal type for the Spending Goal
    /// </summary>
    public required SpendingGoalTypeModel SpendingGoalType { get; init; }
}