namespace Domain.FundGoals.Queries;

/// <summary>
/// Criteria for querying Fund Goals.
/// </summary>
public sealed record FundGoalQuery(FundGoalFilter Filter, FundGoalSort Sort, int Offset, int? Limit);
