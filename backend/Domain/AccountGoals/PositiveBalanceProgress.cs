namespace Domain.AccountGoals;

/// <summary>
/// Projection of positive-balance health for an Account Goal.
/// </summary>
public sealed class PositiveBalanceProgress
{
    /// <summary>
    /// Current Account balance.
    /// </summary>
    public decimal CurrentBalance { get; }

    /// <summary>
    /// True when the current balance is strictly greater than zero.
    /// </summary>
    public bool IsSatisfied => CurrentBalance > 0;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal PositiveBalanceProgress(decimal currentBalance)
    {
        CurrentBalance = currentBalance;
    }
}
