namespace Models.Goals;

/// <summary>
/// Model representing a request to retrieve a specific Goal
/// </summary>
public class GetGoalModel
{
    /// <summary>
    /// Fund ID for the Goal
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Accounting Period ID for the Goal
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }
}