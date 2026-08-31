namespace Models.FundGoals;

/// <summary>
/// Status of a Fund balance relative to its ending-balance bounds.
/// </summary>
public enum FundGoalEndingBalanceStatusModel
{
    /// <summary>
    /// The balance is below the configured minimum.
    /// </summary>
    BelowMinimum,

    /// <summary>
    /// The balance satisfies the configured bounds.
    /// </summary>
    WithinRange,

    /// <summary>
    /// The balance is above the configured maximum.
    /// </summary>
    AboveMaximum,
}
