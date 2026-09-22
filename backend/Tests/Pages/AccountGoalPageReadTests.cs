using Models;
using Models.AccountGoals;
using Models.AccountingPeriods;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Infrastructure;

namespace Tests.Pages;

/// <summary>
/// Characterizes the account goal workspace, detail, and trends reads.
/// </summary>
public sealed class AccountGoalPageReadTests
{
    /// <summary>
    /// Joins workspace goals to progress for the selected period.
    /// </summary>
    [Fact]
    public async Task WorkspaceReturnsMatchingGoalsAndProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?limit=500");
        CollectionModel<AccountGoalModel> goals = await test.Api.GetAsync<CollectionModel<AccountGoalModel>>(
            $"/account-goals?filter.accountingPeriodIds={july.Id}");
        IReadOnlyCollection<AccountGoalProgressResultModel> progresses = await test.Api.GetAsync<IReadOnlyCollection<AccountGoalProgressResultModel>>(
            $"/account-goals/progress/{july.Id}");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        AccountGoalModel goal = Assert.Single(goals.Items, item => item.Account.Id == cash.Id);
        Assert.Contains(progresses, item => item.AccountGoalId == goal.Id && item.Progress.EndingBalance.CurrentBalance == 100m);
    }

    /// <summary>
    /// Keeps copied goal configuration available across the two periods shown in trends.
    /// </summary>
    [Fact]
    public async Task TrendsReturnsGoalHistoryAcrossPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        CollectionModel<AccountGoalModel> goals = await test.Api.GetAsync<CollectionModel<AccountGoalModel>>(
            $"/account-goals?filter.accountIds={cash.Id}&filter.accountingPeriodIds={july.Id}&filter.accountingPeriodIds={august.Id}");
        IReadOnlyCollection<AccountGoalProgressResultModel> julyProgress = await test.Api.GetAsync<IReadOnlyCollection<AccountGoalProgressResultModel>>(
            $"/account-goals/progress/{july.Id}");
        IReadOnlyCollection<AccountGoalProgressResultModel> augustProgress = await test.Api.GetAsync<IReadOnlyCollection<AccountGoalProgressResultModel>>(
            $"/account-goals/progress/{august.Id}");

        Assert.Equal(2, goals.TotalCount);
        Assert.All(goals.Items, goal => Assert.Equal(cash.Id, goal.Account.Id));
        Assert.Contains(julyProgress, item => goals.Items.Any(goal => goal.Id == item.AccountGoalId));
        Assert.Contains(augustProgress, item => goals.Items.Any(goal => goal.Id == item.AccountGoalId));
    }

    /// <summary>
    /// Keeps account goal configuration and its calculated progress together on the detail page.
    /// </summary>
    [Fact]
    public async Task DetailReturnsGoalAndItsCurrentProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        AccountGoalModel goal = await test.Api.GetAsync<AccountGoalModel>(
            $"/account-goals/account/{cash.Id}?accountingPeriodId={july.Id}");
        AccountGoalProgressModel progress = await test.Api.GetAsync<AccountGoalProgressModel>(
            $"/account-goals/{goal.Id}/progress/{july.Id}");

        Assert.Equal(cash.Id, goal.Account.Id);
        Assert.Equal(100m, progress.EndingBalance.CurrentBalance);
        Assert.True(progress.IsSatisfied);
    }
}
