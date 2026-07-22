namespace Domain.FundPlans.Queries;

/// <summary>
/// Criteria for querying Fund Plans.
/// </summary>
public sealed record FundPlanQuery(FundPlanFilter Filter, FundPlanSort Sort, int Offset, int? Limit);