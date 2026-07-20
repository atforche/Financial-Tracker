namespace Models.FundPlans;

/// <summary>
/// Status of a balance relative to its ending target.
/// </summary>
public enum EndingBalanceStatusModel
{
    /// <summary>
    /// The balance is below target.
    /// </summary>
    BelowTarget,

    /// <summary>
    /// The balance equals target.
    /// </summary>
    AtTarget,

    /// <summary>
    /// The balance is above target.
    /// </summary>
    AboveTarget,
}