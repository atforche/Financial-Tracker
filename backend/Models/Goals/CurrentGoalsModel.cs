namespace Models.Goals;

/// <summary>
/// Model representing the current Goals page response.
/// </summary>
public class CurrentGoalsModel
{
    /// <summary>
    /// Latest Accounting Period identifier, when available.
    /// </summary>
    public required Guid? AccountingPeriodId { get; init; }

    /// <summary>
    /// Latest Accounting Period name, when available.
    /// </summary>
    public required string? AccountingPeriodName { get; init; }

    /// <summary>
    /// Available Fund Names for the current snapshot filters.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Current aggregate goal summary for the latest Accounting Period.
    /// </summary>
    public required CurrentGoalsSummaryModel Summary { get; init; }

    /// <summary>
    /// Current per-fund goal snapshot rows.
    /// </summary>
    public required IReadOnlyCollection<CurrentGoalModel> Goals { get; init; }
}