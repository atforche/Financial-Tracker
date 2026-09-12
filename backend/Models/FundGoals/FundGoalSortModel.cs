namespace Models.FundGoals;

/// <summary>
/// Available ordering options for Fund Goals.
/// </summary>
public enum FundGoalSortModel
{
    /// <summary>
    /// Sorts by Fund name.
    /// </summary>
    Fund,

    /// <summary>
    /// Sorts by Fund name descending.
    /// </summary>
    FundDescending,

    /// <summary>
    /// Sorts by planned monthly contribution.
    /// </summary>
    PlannedMonthlyContribution,

    /// <summary>
    /// Sorts by planned monthly contribution descending.
    /// </summary>
    PlannedMonthlyContributionDescending,

    /// <summary>
    /// Sorts by minimum ending balance.
    /// </summary>
    MinimumEndingBalance,

    /// <summary>
    /// Sorts by minimum ending balance descending.
    /// </summary>
    MinimumEndingBalanceDescending,

    /// <summary>
    /// Sorts by maximum ending balance.
    /// </summary>
    MaximumEndingBalance,

    /// <summary>
    /// Sorts by maximum ending balance descending.
    /// </summary>
    MaximumEndingBalanceDescending,

}
