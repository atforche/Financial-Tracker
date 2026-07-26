namespace Domain.FundGoals.Queries;

/// <summary>
/// Supported Fund Goal sort orders.
/// </summary>
public enum FundGoalSort
{
    /// <summary>
    /// Sorts Fund Goals by Fund name in ascending order.
    /// </summary>
    Fund,

    /// <summary>
    /// Sorts Fund Goals by Fund name in descending order.
    /// </summary>
    FundDescending,
}