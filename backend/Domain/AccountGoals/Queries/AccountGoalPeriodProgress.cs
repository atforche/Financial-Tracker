namespace Domain.AccountGoals.Queries;

/// <summary>
/// Account Goal paired with its progress in the goal's Accounting Period.
/// </summary>
public sealed record AccountGoalPeriodProgress(AccountGoal Goal, AccountGoalProgress Progress);
