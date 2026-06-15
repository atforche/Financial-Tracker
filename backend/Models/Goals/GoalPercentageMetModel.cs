namespace Models.Goals;

/// <summary>
/// Model representing the percentage of a goal that have been met.
/// </summary>
public class GoalPercentageMetModel
{
    /// <summary>
    /// Total number of goals.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Number of goals that have been met.
    /// </summary>
    public required int MetCount { get; init; }

    /// <summary>
    /// Percentage of goals that have been met.
    /// </summary>
    public required decimal PercentageMet { get; init; }
}