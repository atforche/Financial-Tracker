namespace Domain.FundGoals;

/// <summary>
/// Projection of ending-balance progress for a Fund Goal.
/// </summary>
public sealed class FundGoalEndingBalanceProgress
{
    /// <summary>
    /// Current ending balance.
    /// </summary>
    public decimal EndingBalance { get; }

    /// <summary>
    /// Configured minimum ending balance.
    /// </summary>
    public decimal MinimumBalance { get; }

    /// <summary>
    /// Configured maximum ending balance.
    /// </summary>
    public decimal? MaximumBalance { get; }

    /// <summary>
    /// Nonnegative amount below the configured minimum.
    /// </summary>
    public decimal AmountBelowMinimum => Math.Max(MinimumBalance - EndingBalance, 0);

    /// <summary>
    /// Nonnegative amount above the configured maximum.
    /// </summary>
    public decimal AmountAboveMaximum => MaximumBalance is decimal maximum
        ? Math.Max(EndingBalance - maximum, 0)
        : 0;

    /// <summary>
    /// Relationship between the ending balance and configured bounds.
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
        decimal endingBalance,
        decimal minimumBalance,
        decimal? maximumBalance)
    {
        EndingBalance = endingBalance;
        MinimumBalance = minimumBalance;
        MaximumBalance = maximumBalance;
    }
}

/// <summary>
/// Status of an ending balance relative to its configured bounds.
/// </summary>
public enum FundGoalEndingBalanceStatus
{
    /// <summary>
    /// The ending balance is below the configured minimum.
    /// </summary>
    BelowMinimum,

    /// <summary>
    /// The ending balance satisfies the configured bounds.
    /// </summary>
    WithinRange,

    /// <summary>
    /// The ending balance is above the configured maximum.
    /// </summary>
    AboveMaximum,
}
