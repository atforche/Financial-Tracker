namespace Domain.AccountGoals;

/// <summary>
/// Projection comparing an Account's financial state with its Account Goal.
/// </summary>
public sealed class AccountGoalProgress
{
    /// <summary>
    /// Ending-balance progress, with a default minimum of zero.
    /// </summary>
    public AccountGoalEndingBalanceProgress EndingBalance { get; }

    /// <summary>
    /// True when the balance satisfies every ending-balance bound.
    /// </summary>
    public bool IsSatisfied => EndingBalance.Status == AccountGoalEndingBalanceStatus.WithinRange;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountGoalProgress(
        AccountGoalEndingBalanceProgress endingBalance)
    {
        EndingBalance = endingBalance;
    }
}
