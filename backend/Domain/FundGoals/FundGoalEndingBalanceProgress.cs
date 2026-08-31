namespace Domain.FundGoals;

/// <summary>
/// Projection of ending-balance progress for a Fund Goal.
/// </summary>
public sealed class FundGoalEndingBalanceProgress
{
    /// <summary>
    /// Current available balance.
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
    public FundGoalEndingBalanceStatus Status => AmountBelowMinimum > 0
        ? FundGoalEndingBalanceStatus.BelowMinimum
        : AmountAboveMaximum > 0
            ? FundGoalEndingBalanceStatus.AboveMaximum
            : FundGoalEndingBalanceStatus.WithinRange;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundGoalEndingBalanceProgress(
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
/// Status of a current balance relative to its configured ending-balance bounds.
/// </summary>
public enum FundGoalEndingBalanceStatus
{
    /// <summary>
    /// The current balance is below the configured minimum.
    /// </summary>
    BelowMinimum,

    /// <summary>
    /// The current balance satisfies the configured bounds.
    /// </summary>
    WithinRange,

    /// <summary>
    /// The current balance is above the configured maximum.
    /// </summary>
    AboveMaximum,
}
