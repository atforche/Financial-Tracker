namespace Domain.AccountGoals;

/// <summary>
/// Service for calculating Account Goal progress from financial facts.
/// </summary>
public static class AccountGoalProgressService
{
    /// <summary>
    /// Calculates progress for an Account Goal in an Accounting Period.
    /// </summary>
    public static AccountGoalProgress Calculate(
        decimal currentBalance,
        decimal? minimumEndingBalance,
        decimal? maximumEndingBalance)
    {
        EndingBalanceProgress? endingBalance = minimumEndingBalance != null || maximumEndingBalance != null
            ? new EndingBalanceProgress(currentBalance, minimumEndingBalance, maximumEndingBalance)
            : null;
        return new AccountGoalProgress(
            new PositiveBalanceProgress(currentBalance),
            endingBalance);
    }
}
