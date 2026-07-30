namespace Models.FundGoals;

/// <summary>
/// Query parameters for retrieving Fund Goals.
/// </summary>
public sealed class FundGoalQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Gets the optional Fund Goal filter.
    /// </summary>
    public FundGoalFilterModel? Filter { get; init; }

    /// <summary>
    /// Gets the optional Fund Goal ordering.
    /// </summary>
    public FundGoalSortModel? Sort { get; init; }
}