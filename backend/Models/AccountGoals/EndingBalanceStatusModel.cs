namespace Models.AccountGoals;

/// <summary>
/// Status of an Account balance relative to configured ending-balance bounds.
/// </summary>
public enum EndingBalanceStatusModel
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
