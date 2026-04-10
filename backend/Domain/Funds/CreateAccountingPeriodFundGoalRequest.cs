namespace Domain.Funds;

/// <summary>
/// Request to create a Fund Goal as part of creating a new Accounting Period
/// </summary>
public record CreateAccountingPeriodFundGoalRequest
{
    /// <summary>
    /// The Fund to create a goal for
    /// </summary>
    public required Fund Fund { get; init; }

    /// <summary>
    /// Goal amount for this Fund in the new Accounting Period
    /// </summary>
    public required decimal GoalAmount { get; init; }
}
