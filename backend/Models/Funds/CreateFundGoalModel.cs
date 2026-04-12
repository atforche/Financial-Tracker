namespace Models.Funds;

/// <summary>
/// Model representing a request to create a Fund Goal
/// </summary>
public class CreateFundGoalModel
{
    /// <summary>
    /// Accounting Period that the Fund Goal is associated with
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Goal type for the Fund Goal
    /// </summary>
    public required FundGoalTypeModel GoalType { get; init; }

    /// <summary>
    /// Goal amount for the Fund Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}