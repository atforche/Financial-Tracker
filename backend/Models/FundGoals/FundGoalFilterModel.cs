namespace Models.FundGoals;

/// <summary>
/// Filters Fund Goals by related resources.
/// </summary>
public sealed class FundGoalFilterModel
{
    /// <summary>
    /// Gets the Fund IDs to include.
    /// </summary>
    public IReadOnlyCollection<Guid>? FundIds { get; init; }

    /// <summary>
    /// Gets the Accounting Period IDs to include.
    /// </summary>
    public IReadOnlyCollection<Guid>? AccountingPeriodIds { get; init; }

    /// <summary>
    /// Gets whether onboarded fundGoals with no Accounting Period should be included.
    /// </summary>
    public bool? IncludeOnboarded { get; init; }
}