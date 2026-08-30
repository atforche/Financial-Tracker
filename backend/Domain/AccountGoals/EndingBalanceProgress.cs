namespace Domain.AccountGoals;

/// <summary>
/// Projection of ending-balance progress for an Account Goal.
/// </summary>
public sealed class EndingBalanceProgress
{
    /// <summary>
    /// Current Account balance.
    /// </summary>
    public decimal CurrentBalance { get; }

    /// <summary>
    /// Configured minimum ending balance.
    /// </summary>
    public decimal? MinimumBalance { get; }

    /// <summary>
    /// Configured maximum ending balance.
    /// </summary>
    public decimal? MaximumBalance { get; }

    /// <summary>
    /// Nonnegative amount below the configured minimum.
    /// </summary>
    public decimal AmountBelowMinimum => MinimumBalance is decimal minimum
        ? Math.Max(minimum - CurrentBalance, 0)
        : 0;

    /// <summary>
    /// Nonnegative amount above the configured maximum.
    /// </summary>
    public decimal AmountAboveMaximum => MaximumBalance is decimal maximum
        ? Math.Max(CurrentBalance - maximum, 0)
        : 0;

    /// <summary>
    /// Relationship between the current balance and configured bounds.
    /// </summary>
    public EndingBalanceStatus Status =>
        AmountBelowMinimum > 0 ? EndingBalanceStatus.BelowMinimum
        : AmountAboveMaximum > 0 ? EndingBalanceStatus.AboveMaximum
        : EndingBalanceStatus.WithinRange;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal EndingBalanceProgress(
        decimal currentBalance,
        decimal? minimumBalance,
        decimal? maximumBalance)
    {
        CurrentBalance = currentBalance;
        MinimumBalance = minimumBalance;
        MaximumBalance = maximumBalance;
    }
}

/// <summary>
/// Status of an Account balance relative to configured ending-balance bounds.
/// </summary>
public enum EndingBalanceStatus
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
