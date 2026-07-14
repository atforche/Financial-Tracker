namespace Models.Goals;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Assignment Goals
/// </summary>
public class AssignmentGoalQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filters to apply to the results.
    /// </summary>
    public GoalFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public AssignmentGoalSortModel? Sort { get; init; }
}