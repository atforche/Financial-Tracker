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
        decimal? maximumEndingBalance) => new(new AccountGoalEndingBalanceProgress(
            currentBalance,
            minimumEndingBalance ?? 0m,
            maximumEndingBalance));
}
