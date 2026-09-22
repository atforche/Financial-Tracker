using Models;
using Models.AccountingPeriods;
using Models.FundGoals;
using Tests.AccountingPeriods;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Pages;

/// <summary>
/// Characterizes the fund goal workspace, detail, and trends reads.
/// </summary>
public sealed class FundGoalPageReadTests
{
    /// <summary>
    /// Joins workspace goals to progress for the selected period.
    /// </summary>
    [Fact]
    public async Task WorkspaceReturnsMatchingGoalsAndProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?limit=500");
        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.accountingPeriodIds={july.Id}");
        IReadOnlyCollection<FundGoalProgressResultModel> progresses = await test.Api.GetAsync<IReadOnlyCollection<FundGoalProgressResultModel>>(
            $"/fund-goals/progress/{july.Id}");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        FundGoalModel goal = Assert.Single(goals.Items, item => item.Fund.Id == groceries.Id);
        Assert.Contains(progresses, item => item.FundGoalId == goal.Id);
    }

    /// <summary>
    /// Keeps copied fund goals available across the two periods shown in trends.
    /// </summary>
    [Fact]
    public async Task TrendsReturnsGoalHistoryAcrossPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.fundIds={groceries.Id}&filter.accountingPeriodIds={july.Id}&filter.accountingPeriodIds={august.Id}");
        IReadOnlyCollection<FundGoalProgressResultModel> julyProgress = await test.Api.GetAsync<IReadOnlyCollection<FundGoalProgressResultModel>>(
            $"/fund-goals/progress/{july.Id}");
        IReadOnlyCollection<FundGoalProgressResultModel> augustProgress = await test.Api.GetAsync<IReadOnlyCollection<FundGoalProgressResultModel>>(
            $"/fund-goals/progress/{august.Id}");

        Assert.Equal(2, goals.TotalCount);
        Assert.All(goals.Items, goal => Assert.Equal(groceries.Id, goal.Fund.Id));
        Assert.Contains(julyProgress, item => goals.Items.Any(goal => goal.Id == item.FundGoalId));
        Assert.Contains(augustProgress, item => goals.Items.Any(goal => goal.Id == item.FundGoalId));
    }

    /// <summary>
    /// Keeps fund goal configuration and its calculated progress together on the detail page.
    /// </summary>
    [Fact]
    public async Task DetailReturnsGoalAndItsCurrentProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        FundGoalModel goal = await test.Api.GetAsync<FundGoalModel>(
            $"/fund-goals/fund/{groceries.Id}?accountingPeriodId={july.Id}");
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>(
            $"/fund-goals/{goal.Id}/progress/{july.Id}");

        Assert.Equal(groceries.Id, goal.Fund.Id);
        Assert.Equal(0m, progress.EndingBalance.EndingBalance);
    }
}
