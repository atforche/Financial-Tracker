namespace Domain.FundGoals;

/// <summary>
/// Projection of post-assignment funded-balance progress for a Fund Goal.
/// </summary>
public sealed class FundedBalanceProgress
{
    /// <summary>
    /// Opening available balance plus assignments made during the Accounting Period.
    /// </summary>
    public decimal Balance { get; }

    /// <summary>
    /// Configured minimum funded balance.
    /// </summary>
    public decimal? MinimumBalance { get; }

    /// <summary>
    /// Configured maximum funded balance.
    /// </summary>
    public decimal? MaximumBalance { get; }

    /// <summary>
    /// Nonnegative amount by which the balance is below the configured minimum.
    /// </summary>
    public decimal AmountBelowMinimum => MinimumBalance is decimal minimum
        ? Math.Max(minimum - Balance, 0)
        : 0;

    /// <summary>
    /// Nonnegative amount by which the balance is above the configured maximum.
    /// </summary>
    public decimal AmountAboveMaximum => MaximumBalance is decimal maximum
        ? Math.Max(Balance - maximum, 0)
        : 0;

    /// <summary>
    /// Relationship between the funded balance and the configured bounds.
    /// </summary>
    public FundedBalanceStatus Status => AmountBelowMinimum > 0
        ? FundedBalanceStatus.BelowMinimum
        : AmountAboveMaximum > 0
            ? FundedBalanceStatus.AboveMaximum
            : FundedBalanceStatus.WithinRange;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundedBalanceProgress(decimal balance, decimal? minimumBalance, decimal? maximumBalance)
    {
        Balance = balance;
        MinimumBalance = minimumBalance;
        MaximumBalance = maximumBalance;
    }
}

/// <summary>
/// Status of a funded balance relative to its configured bounds.
/// </summary>
public enum FundedBalanceStatus
{
    /// <summary>
    /// The balance is below the configured minimum.
    /// </summary>
    BelowMinimum,

    /// <summary>
    /// The balance satisfies all configured bounds.
    /// </summary>
    WithinRange,

    /// <summary>
    /// The balance is above the configured maximum.
    /// </summary>
    AboveMaximum,
}
