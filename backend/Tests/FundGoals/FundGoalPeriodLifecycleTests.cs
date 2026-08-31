using System.Net;
using Models.AccountingPeriods;
using Models.FundGoals;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.FundGoals;

/// <summary>
/// Covers Fund Goal configuration copied into subsequent Accounting Periods.
/// </summary>
public sealed class FundGoalPeriodLifecycleTests
{
    /// <summary>
    /// Copies configured Fund Goals forward and exposes them through period-scoped lookup and progress APIs.
    /// </summary>
    [Fact]
    public async Task CreatingNextPeriodCopiesGoalConfigurationAndExposesAllProgresses()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            RegularContribution = 50m,
            MinimumEndingBalance = 100m,
            MaximumEndingBalance = 200m
        });
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        FundGoalModel copied = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/fund/{groceries.Id}?accountingPeriodId={august.Id}");
        IReadOnlyCollection<FundGoalProgressResultModel> progresses = await test.Api.GetAsync<IReadOnlyCollection<FundGoalProgressResultModel>>(
            $"/fund-goals/progress/{august.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/fund-goals/progress/{Guid.NewGuid()}");

        Assert.Equal(50m, copied.RegularContribution);
        Assert.Equal(100m, copied.MinimumEndingBalance);
        Assert.Equal(200m, copied.MaximumEndingBalance);
        Assert.Contains(progresses, item => item.FundGoalId == copied.Id && item.Progress.Contribution != null);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Calculates period progress from posted activity while availability retains pending effects.
    /// </summary>
    [Fact]
    public async Task ProgressReflectsPostedAndPendingFundActivity()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            RegularContribution = 50m,
            MinimumEndingBalance = 25m,
            MaximumEndingBalance = 100m
        });
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(60m).From("Employer").To(cash, groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        _ = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 20)).For(20m).From(groceries).To(dining).CreateAsync();

        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{groceries.Goal.Id}/progress/{july.Id}");
        AccountingPeriodWithBalanceModel period = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}");
        FundGoalAvailabilitySnapshot availability = await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal);

        Assert.True(progress.AvailableBalance.IsSatisfied);
        Assert.Equal(40m, progress.AvailableBalance.CurrentBalance);
        Assert.Equal(40m, availability.Posted);
        Assert.Equal(20m, availability.IncludingPending);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(50m, progress.Contribution.TargetAmount);
        Assert.Equal(60m, progress.Contribution.AssignedAmount);
        Assert.Equal(60m, period.ActualGoalContributions);
        Assert.NotNull(progress.EndingBalance);
        Assert.Equal(40m, progress.EndingBalance.CurrentBalance);
        Assert.Equal(FundGoalEndingBalanceStatusModel.WithinRange, progress.EndingBalance.Status);
    }
}
