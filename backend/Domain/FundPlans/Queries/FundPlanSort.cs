namespace Domain.FundPlans.Queries;

/// <summary>
/// Supported Fund Plan sort orders.
/// </summary>
public enum FundPlanSort
{
    /// <summary>
    /// Sorts Fund Plans by Fund name in ascending order.
    /// </summary>
    Fund,

    /// <summary>
    /// Sorts Fund Plans by Fund name in descending order.
    /// </summary>
    FundDescending,
}