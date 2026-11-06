using System.Net;
using Models.AccountGoals;

namespace Tests.AccountGoals;

/// <summary>
/// Covers Account Goal REST configuration and progress endpoints.
/// </summary>
public sealed class AccountGoalEndpointTests
{
    /// <summary>
    /// Returns an Account's goals and progress across a period range, oldest first, and rejects unknown periods.
    /// </summary>
    [Fact]
    public async Task HistoryReturnsGoalsAcrossPeriodRange()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle account = await test.Accounts.Onboard("Spending").CreateAsync();
        _ = await test.Accounts.Onboard("Other").CreateAsync();
        _ = await test.Periods.Create(2026, 8).CreateAsync();
        AccountingPeriodHandle september = await test.Periods.Create(2026, 9).CreateAsync();
        AccountingPeriodHandle october = await test.Periods.Create(2026, 10).CreateAsync();

        IReadOnlyList<AccountGoalPeriodProgressModel> history = await test.Api.GetAsync<List<AccountGoalPeriodProgressModel>>(
            $"/account-goals/account/{account.Id}/history?range.start={september.Id}&range.end={october.Id}");

        Assert.Equal([9, 10], history.Select(item => item.Goal.AccountingPeriod!.Month));
        Assert.All(history, item => Assert.Equal(account.Id, item.Goal.Account.Id));

        HttpResponseMessage invalid = await test.Api.GetResponseAsync(
            $"/account-goals/account/{account.Id}/history?range.start={Guid.NewGuid()}&range.end={october.Id}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
    }

    /// <summary>
    /// Exposes Account Goal configuration, and progress through the REST API.
    /// </summary>
    [Fact]
    public async Task EndpointsExposeConfigurationAndProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle account = await test.Accounts.Onboard("Checking").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle period = await test.Periods.Create(2026, 8).CreateAsync();

        AccountGoalModel goal = await test.Api.GetAccountGoalAsync(account.Id, period.Id);
        Assert.Equal(0m, goal.MinimumEndingBalance);
        AccountGoalModel updated = await test.Api.PostAsync<UpdateAccountGoalModel, AccountGoalModel>(
            $"/account-goals/{goal.Id}",
            new UpdateAccountGoalModel
            {
                MinimumEndingBalance = 50m,
                MaximumEndingBalance = 150m,
            });

        Assert.Equal(50m, updated.MinimumEndingBalance);
        Assert.Equal(150m, updated.MaximumEndingBalance);

        IReadOnlyList<AccountGoalPeriodProgressModel> history = await test.Api.GetAsync<List<AccountGoalPeriodProgressModel>>(
            $"/account-goals/account/{account.Id}/history?range.start={period.Id}&range.end={period.Id}");
        AccountGoalPeriodProgressModel entry = Assert.Single(history);
        Assert.Equal(goal.Id, entry.Goal.Id);
        Assert.Equal(100m, entry.Progress.EndingBalance.CurrentBalance);
        Assert.True(entry.Progress.IsSatisfied);
        Assert.Equal(AccountGoalEndingBalanceStatusModel.WithinRange, entry.Progress.EndingBalance.Status);
    }

    /// <summary>
    /// Rejects invalid Account Goal updates and returns not-found for missing resources.
    /// </summary>
    [Fact]
    public async Task EndpointsValidateUpdatesAndMissingResources()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle account = await test.Accounts.Onboard("Checking").CreateAsync();
        AccountingPeriodHandle period = await test.Periods.Create(2026, 8).CreateAsync();
        AccountGoalModel goal = await test.Api.GetAccountGoalAsync(account.Id, period.Id);

        using HttpResponseMessage invalidUpdate = await test.Api.PostResponseAsync(
            $"/account-goals/{goal.Id}",
            new UpdateAccountGoalModel
            {
                MinimumEndingBalance = 200m,
                MaximumEndingBalance = 100m,
            });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidUpdate.StatusCode);
    }
}
