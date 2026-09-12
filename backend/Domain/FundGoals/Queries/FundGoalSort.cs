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

    /// <summary>
    /// Sorts Fund Goals by planned monthly contribution in ascending order.
    /// </summary>
    PlannedMonthlyContribution,

    /// <summary>
    /// Sorts Fund Goals by planned monthly contribution in descending order.
    /// </summary>
    PlannedMonthlyContributionDescending,

    /// <summary>
    /// Sorts Fund Goals by minimum ending balance in ascending order.
    /// </summary>
    MinimumEndingBalance,

    /// <summary>
    /// Sorts Fund Goals by minimum ending balance in descending order.
    /// </summary>
    MinimumEndingBalanceDescending,

    /// <summary>
    /// Sorts Fund Goals by maximum ending balance in ascending order.
    /// </summary>
    MaximumEndingBalance,

    /// <summary>
    /// Sorts Fund Goals by maximum ending balance in descending order.
    /// </summary>
    MaximumEndingBalanceDescending,

}
