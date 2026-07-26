namespace Domain.FundGoals;

/// <summary>
/// Projection of a Fund's available-balance health for a Fund Goal.
/// </summary>
public sealed class AvailableBalanceProgress
{
    /// <summary>
    /// Current available balance.
    /// </summary>
    public decimal CurrentBalance { get; }

    /// <summary>
    /// Minimum allowed available balance.
    /// </summary>
    public decimal MinimumBalance { get; } = 0;

    /// <summary>
    /// Nonnegative amount required to restore the minimum allowed balance.
    /// </summary>
    public decimal Shortfall => Math.Max(MinimumBalance - CurrentBalance, 0);

    /// <summary>
    /// True when the current available balance is not below zero.
    /// </summary>
    public bool IsSatisfied => CurrentBalance >= MinimumBalance;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AvailableBalanceProgress(decimal currentBalance)
    {
        CurrentBalance = currentBalance;
    }
}