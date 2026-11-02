using Domain.AccountGoals;
using Domain.FundGoals;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Counts of an Accounting Period's Fund Goals and Account Goals by their progress.
/// </summary>
/// <param name="FundGoalCount">The number of Fund Goals.</param>
/// <param name="ContributionGoalCount">The number of Fund Goals with a planned contribution.</param>
/// <param name="ContributedGoalCount">The number of Fund Goals whose expected contribution has been assigned.</param>
/// <param name="FundGoalsBelowMinimumCount">The number of Fund Goals whose ending balance is below their minimum.</param>
/// <param name="FundGoalsAboveMaximumCount">The number of Fund Goals whose ending balance is above their maximum.</param>
/// <param name="AccountGoalCount">The number of Account Goals.</param>
/// <param name="AccountGoalsBelowMinimumCount">The number of Account Goals whose ending balance is below their minimum.</param>
/// <param name="AccountGoalsAboveMaximumCount">The number of Account Goals whose ending balance is above their maximum.</param>
public sealed record AccountingPeriodGoalSummary(
    int FundGoalCount,
    int ContributionGoalCount,
    int ContributedGoalCount,
    int FundGoalsBelowMinimumCount,
    int FundGoalsAboveMaximumCount,
    int AccountGoalCount,
    int AccountGoalsBelowMinimumCount,
    int AccountGoalsAboveMaximumCount)
{
    /// <summary>
    /// Summarizes the provided Fund Goal and Account Goal progress.
    /// </summary>
    internal static AccountingPeriodGoalSummary From(
        IReadOnlyCollection<FundGoalProgress> fundGoals,
        IReadOnlyCollection<AccountGoalProgress> accountGoals) => new(
        fundGoals.Count,
        fundGoals.Count(goal => goal.Contribution != null),
        fundGoals.Count(goal => goal.Contribution?.IsSatisfied == true),
        fundGoals.Count(goal => goal.EndingBalance.Status == FundGoalEndingBalanceStatus.BelowMinimum),
        fundGoals.Count(goal => goal.EndingBalance.Status == FundGoalEndingBalanceStatus.AboveMaximum),
        accountGoals.Count,
        accountGoals.Count(goal => goal.EndingBalance.Status == AccountGoalEndingBalanceStatus.BelowMinimum),
        accountGoals.Count(goal => goal.EndingBalance.Status == AccountGoalEndingBalanceStatus.AboveMaximum));
}
