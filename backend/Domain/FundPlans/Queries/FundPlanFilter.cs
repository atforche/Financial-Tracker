namespace Domain.FundPlans.Queries;

/// <summary>
/// Criteria used to filter Fund Plans.
/// </summary>
public sealed record FundPlanFilter(
    IReadOnlyCollection<Guid> FundIds,
    IReadOnlyCollection<Guid> AccountingPeriodIds,
    bool? IncludeOnboarded);