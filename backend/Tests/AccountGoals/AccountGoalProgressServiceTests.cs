using Domain.AccountGoals;

namespace Tests.AccountGoals;

/// <summary>
/// Covers pure Account Goal progress rules.
/// </summary>
public sealed class AccountGoalProgressServiceTests
{
    /// <summary>
    /// Treats zero as not satisfying the positive-balance rule.
    /// </summary>
    [Fact]
    public void CalculateTreatsZeroAsNotPositive()
    {
        AccountGoalProgress progress = AccountGoalProgressService.Calculate(0m, null, null);

        Assert.False(progress.PositiveBalance.IsSatisfied);
        Assert.False(progress.IsSatisfied);
        Assert.Equal(0m, progress.PositiveBalance.CurrentBalance);
        Assert.Null(progress.EndingBalance);
    }

    /// <summary>
    /// Exposes ending-balance status and variance amounts for configured bounds.
    /// </summary>
    [Fact]
    public void CalculateReportsEndingBalanceBoundStatuses()
    {
        AccountGoalProgress below = AccountGoalProgressService.Calculate(80m, 100m, 200m);
        AccountGoalProgress within = AccountGoalProgressService.Calculate(150m, 100m, 200m);
        AccountGoalProgress above = AccountGoalProgressService.Calculate(250m, 100m, 200m);

        Assert.Equal(AccountGoalEndingBalanceStatus.BelowMinimum, below.EndingBalance!.Status);
        Assert.Equal(20m, below.EndingBalance.AmountBelowMinimum);
        Assert.Equal(AccountGoalEndingBalanceStatus.WithinRange, within.EndingBalance!.Status);
        Assert.Equal(AccountGoalEndingBalanceStatus.AboveMaximum, above.EndingBalance!.Status);
        Assert.Equal(50m, above.EndingBalance.AmountAboveMaximum);
        Assert.True(within.IsSatisfied);
        Assert.False(below.IsSatisfied);
        Assert.False(above.IsSatisfied);
    }
}
