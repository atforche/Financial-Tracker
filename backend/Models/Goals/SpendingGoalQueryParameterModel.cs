namespace Models.Goals;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Spending Goals
/// </summary>
public class SpendingGoalQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filters to apply to the results.
    /// </summary>
    public GoalFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public SpendingGoalSortModel? Sort { get; init; }
}