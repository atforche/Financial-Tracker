using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Record representing a request to create a <see cref="FundGoal"/>
/// </summary>
public record CreateFundGoalRequest
{
    /// <summary>
    /// Fund for this Fund Goal
    /// </summary>
    public Fund Fund { get; init; } = null!;

    /// <summary>
    /// Accounting Period for this Fund Goal
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; } = null!;

    /// <summary>
    /// Target goal amount for the Fund Goal
    /// </summary>
    public required decimal GoalAmount { get; init; }
}