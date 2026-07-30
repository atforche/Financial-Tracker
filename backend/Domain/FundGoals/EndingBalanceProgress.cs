namespace Domain.FundGoals;

/// <summary>
/// Projection of ending-balance progress for a Fund Goal.
/// </summary>
public sealed class EndingBalanceProgress
{
    /// <summary>
    /// Configured target ending balance.
    /// </summary>
    public decimal TargetBalance { get; }

    /// <summary>
    /// Current available balance.
    /// </summary>
    public decimal CurrentBalance { get; }

    /// <summary>
    /// Current balance minus the target balance.
    /// </summary>
    public decimal Variance => CurrentBalance - TargetBalance;

    /// <summary>
    /// Relationship between the current and target balances.
    /// </summary>
    public EndingBalanceStatus Status => Variance switch
    {
        < 0 => EndingBalanceStatus.BelowTarget,
        > 0 => EndingBalanceStatus.AboveTarget,
        _ => EndingBalanceStatus.AtTarget,
    };

    /// <summary>
    /// Forecast balance at the end of the Accounting Period, when available.
    /// </summary>
    public decimal? ProjectedEndingBalance { get; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal EndingBalanceProgress(decimal targetBalance, decimal currentBalance, decimal? projectedEndingBalance = null)
    {
        TargetBalance = targetBalance;
        CurrentBalance = currentBalance;
        ProjectedEndingBalance = projectedEndingBalance;
    }
}

/// <summary>
/// Status of a current balance relative to its target ending balance.
/// </summary>
public enum EndingBalanceStatus
{
    /// <summary>
    /// The current balance is below the target.
    /// </summary>
    BelowTarget,

    /// <summary>
    /// The current balance equals the target.
    /// </summary>
    AtTarget,

    /// <summary>
    /// The current balance is above the target.
    /// </summary>
    AboveTarget,
}