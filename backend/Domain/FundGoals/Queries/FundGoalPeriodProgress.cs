namespace Domain.FundGoals.Queries;

/// <summary>
/// Fund Goal paired with its progress in the goal's Accounting Period.
/// </summary>
public sealed record FundGoalPeriodProgress(FundGoal Goal, FundGoalProgress Progress);
