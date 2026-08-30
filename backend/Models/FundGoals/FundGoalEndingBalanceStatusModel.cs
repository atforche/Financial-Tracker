namespace Models.FundGoals;

/// <summary>
/// Status of a Fund balance relative to its ending target.
/// </summary>
public enum FundGoalEndingBalanceStatusModel
{
    /// <summary>
    /// The balance is below the target.
    /// </summary>
    BelowTarget,

    /// <summary>
    /// The balance is at the target.
    /// </summary>
    AtTarget,

    /// <summary>
    /// The balance is above the target.
    /// </summary>
    AboveTarget,
}
