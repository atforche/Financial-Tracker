namespace Domain.AccountGoals;

/// <summary>
/// Projection comparing an Account's financial state with its Account Goal.
/// </summary>
public sealed class AccountGoalProgress
{
    /// <summary>
    /// Positive-balance health.
    /// </summary>
    public PositiveBalanceProgress PositiveBalance { get; }

    /// <summary>
    /// Ending-balance progress, or null when no ending-balance bounds are configured.
    /// </summary>
    public AccountGoalEndingBalanceProgress? EndingBalance { get; }

    /// <summary>
    /// True when the balance is positive and satisfies every configured ending-balance bound.
    /// </summary>
    public bool IsSatisfied => PositiveBalance.IsSatisfied
        && (EndingBalance == null || EndingBalance.Status == AccountGoalEndingBalanceStatus.WithinRange);

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountGoalProgress(
        PositiveBalanceProgress positiveBalance,
        AccountGoalEndingBalanceProgress? endingBalance)
    {
        PositiveBalance = positiveBalance;
        EndingBalance = endingBalance;
    }
}
