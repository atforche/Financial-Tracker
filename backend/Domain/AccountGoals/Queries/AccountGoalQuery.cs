namespace Domain.AccountGoals.Queries;

/// <summary>
/// Criteria for querying Account Goals.
/// </summary>
public sealed record AccountGoalQuery(AccountGoalFilter Filter, AccountGoalSort Sort, int Offset, int? Limit);
