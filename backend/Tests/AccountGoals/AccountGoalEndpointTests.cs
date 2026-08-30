using System.Net;
using Models;
using Models.AccountGoals;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Infrastructure;

namespace Tests.AccountGoals;

/// <summary>
/// Covers Account Goal REST configuration and progress endpoints.
/// </summary>
public sealed class AccountGoalEndpointTests
{
    /// <summary>
    /// Exposes Account Goal configuration, filtering, and progress through the REST API.
    /// </summary>
    [Fact]
    public async Task EndpointsExposeConfigurationAndProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle account = await test.Accounts.Onboard("Checking").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle period = await test.Periods.Create(2026, 8).CreateAsync();

        AccountGoalModel goal = await test.Api.GetAsync<AccountGoalModel>(
            $"/account-goals/account/{account.Id}?accountingPeriodId={period.Id}");
        AccountGoalModel updated = await test.Api.PostAsync<UpdateAccountGoalModel, AccountGoalModel>(
            $"/account-goals/{goal.Id}",
            new UpdateAccountGoalModel
            {
                MinimumEndingBalance = 50m,
                MaximumEndingBalance = 150m,
            });

        Assert.Equal(50m, updated.MinimumEndingBalance);
        Assert.Equal(150m, updated.MaximumEndingBalance);

        CollectionModel<AccountGoalModel> filtered = await test.Api.GetAsync<CollectionModel<AccountGoalModel>>(
            $"/account-goals?filter.accountIds={account.Id}&filter.accountingPeriodIds={period.Id}");
        AccountGoalModel filteredGoal = Assert.Single(filtered.Items);
        Assert.Equal(goal.Id, filteredGoal.Id);
        Assert.Equal(1, filtered.TotalCount);

        AccountGoalProgressModel progress = await test.Api.GetAsync<AccountGoalProgressModel>(
            $"/account-goals/{goal.Id}/progress/{period.Id}");
        Assert.Equal(100m, progress.PositiveBalance.CurrentBalance);
        Assert.True(progress.PositiveBalance.IsSatisfied);
        Assert.True(progress.IsSatisfied);
        Assert.Equal(AccountGoalEndingBalanceStatusModel.WithinRange, progress.EndingBalance!.Status);

        IReadOnlyCollection<AccountGoalProgressResultModel> progresses = await test.Api.GetAsync<IReadOnlyCollection<AccountGoalProgressResultModel>>(
            $"/account-goals/progress/{period.Id}");
        Assert.Contains(progresses, result => result.AccountGoalId == goal.Id);
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
        AccountGoalModel goal = await test.Api.GetAsync<AccountGoalModel>(
            $"/account-goals/account/{account.Id}?accountingPeriodId={period.Id}");

        using HttpResponseMessage invalidUpdate = await test.Api.PostResponseAsync(
            $"/account-goals/{goal.Id}",
            new UpdateAccountGoalModel
            {
                MinimumEndingBalance = 200m,
                MaximumEndingBalance = 100m,
            });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidUpdate.StatusCode);

        using HttpResponseMessage missingGoal = await test.Api.GetResponseAsync($"/account-goals/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missingGoal.StatusCode);

        using HttpResponseMessage missingPeriod = await test.Api.GetResponseAsync(
            $"/account-goals/{goal.Id}/progress/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missingPeriod.StatusCode);
    }
}
