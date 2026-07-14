namespace Models.Goals;

/// <summary>
/// Model representing the filters that can be applied when retrieving Goals.
/// </summary>
public class GoalFilterModel
{
    /// <summary>
    /// Accounting Period identifiers to apply to the results.
    /// </summary>
    public IReadOnlyCollection<Guid>? AccountingPeriodIds { get; init; }

    /// <summary>
    /// Fund identifiers to apply to the results.
    /// </summary>
    public IReadOnlyCollection<Guid>? FundIds { get; init; }
}