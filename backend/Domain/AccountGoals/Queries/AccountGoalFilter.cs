namespace Domain.AccountGoals.Queries;

/// <summary>
/// Criteria used to filter Account Goals.
/// </summary>
public sealed record AccountGoalFilter(
    IReadOnlyCollection<Guid> AccountIds,
    IReadOnlyCollection<Guid> AccountingPeriodIds,
    bool? IncludeOnboarded);
