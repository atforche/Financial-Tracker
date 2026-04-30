namespace Models.Goals;

/// <summary>
/// Model representing a request to create a Goal
/// </summary>
public class CreateGoalModel
{
    /// <summary> 
    /// Fund ID that the Goal is associated with
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Accounting Period that the Goal is associated with
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Goal type for the Goal
    /// </summary>
    public required GoalTypeModel GoalType { get; init; }

    /// <summary>
    /// Goal amount for the Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}