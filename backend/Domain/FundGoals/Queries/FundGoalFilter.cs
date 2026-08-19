namespace Domain.FundGoals.Queries;

/// <summary>
/// Criteria used to filter Fund Goals.
/// </summary>
public sealed record FundGoalFilter(
    IReadOnlyCollection<Guid> FundIds,
    IReadOnlyCollection<Guid> AccountingPeriodIds,
    bool? IncludeOnboarded);
