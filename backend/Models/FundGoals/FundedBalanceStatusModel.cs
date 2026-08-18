namespace Models.FundGoals;

/// <summary>
/// Status of a funded balance relative to configured bounds.
/// </summary>
public enum FundedBalanceStatusModel
{
    /// <summary>
    /// The balance is below the minimum.
    /// </summary>
    BelowMinimum,

    /// <summary>
    /// The balance satisfies configured bounds.
    /// </summary>
    WithinRange,

    /// <summary>
    /// The balance is above the maximum.
    /// </summary>
    AboveMaximum,
}
