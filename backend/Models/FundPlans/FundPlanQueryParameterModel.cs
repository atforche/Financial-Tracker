namespace Models.FundPlans;

/// <summary>
/// Query parameters for retrieving Fund Plans.
/// </summary>
public sealed class FundPlanQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Gets the optional Fund Plan filter.
    /// </summary>
    public FundPlanFilterModel? Filter { get; init; }

    /// <summary>
    /// Gets the optional Fund Plan ordering.
    /// </summary>
    public FundPlanSortModel? Sort { get; init; }
}