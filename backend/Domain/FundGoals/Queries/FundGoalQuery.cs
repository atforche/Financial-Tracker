namespace Domain.FundGoals.Queries;

/// <summary>
/// Criteria for querying Fund Goals.
/// </summary>
public sealed record FundGoalQuery(FundGoalFilter Filter, int Offset, int? Limit);
