namespace Models.Funds;

/// <summary>
/// Model representing a request to update a Fund Goal
/// </summary>
public class UpdateFundGoalModel
{
    /// <summary>
    /// Goal type for the Fund Goal
    /// </summary>
    public required FundGoalTypeModel GoalType { get; init; }

    /// <summary>
    /// Goal amount for the Fund Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}