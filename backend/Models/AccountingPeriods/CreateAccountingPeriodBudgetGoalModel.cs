namespace Models.AccountingPeriods;

/// <summary>
/// Model representing a request to create a Fund Goal as part of creating an Accounting Period
/// </summary>
public class CreateAccountingPeriodFundGoalModel
{
    /// <summary>
    /// ID of the Fund to create a goal for
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Goal amount for this Fund in the new Accounting Period
    /// </summary>
    public required decimal GoalAmount { get; init; }
}
