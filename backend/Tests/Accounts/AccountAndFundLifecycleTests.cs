using System.Net;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Covers account and fund resource lifecycle contracts outside transaction setup.
/// </summary>
public sealed class AccountAndFundLifecycleTests
{
    /// <summary>
    /// Updates and deletes accounts while exposing missing-resource behavior.
    /// </summary>
    [Fact]
    public async Task AccountUpdateAndDeleteAsyncReturnResourceContracts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle account = await test.Accounts.Onboard("Checking").WithOpeningBalance(200m).CreateAsync();

        using HttpResponseMessage update = await test.Api.PostResponseAsync($"/accounts/{account.Id}", new UpdateAccountModel { Name = "Main checking" });
        AccountModel updated = await test.Api.GetAsync<AccountModel>($"/accounts/{account.Id}");
        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/accounts/{account.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/accounts/{account.Id}");
        using HttpResponseMessage missingUpdate = await test.Api.PostResponseAsync($"/accounts/{Guid.NewGuid()}", new UpdateAccountModel { Name = "Missing" });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Main checking", updated.Name);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingUpdate.StatusCode);
    }

    /// <summary>
    /// Updates and deletes funds while rejecting an unknown opening period.
    /// </summary>
    [Fact]
    public async Task FundCreateUpdateAndDeleteAsyncValidatePeriodAndResource()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle fund = await test.Funds.Create("Travel").In(july).CreateAsync();

        using HttpResponseMessage invalidPeriod = await test.Api.PostResponseAsync("/funds", new CreateFundModel
        {
            Name = "Invalid",
            Description = "Invalid",
            AccountingPeriodId = Guid.NewGuid()
        });
        using HttpResponseMessage update = await test.Api.PostResponseAsync($"/funds/{fund.Id}", new UpdateFundModel
        {
            Name = "Holiday",
            Description = "Updated"
        });
        FundModel updated = await test.Api.GetAsync<FundModel>($"/funds/{fund.Id}");
        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/funds/{fund.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/funds/{fund.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Holiday", updated.Name);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Rejects Fund Goal bounds that cannot describe a valid funding range.
    /// </summary>
    [Fact]
    public async Task FundCreateAsyncRejectsInvertedFundingBounds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage response = await test.Api.PostResponseAsync("/funds", new CreateFundModel
        {
            Name = "Invalid bounds",
            Description = "Invalid bounds",
            AccountingPeriodId = july.Id,
            MinimumEndingBalance = 200m,
            MaximumEndingBalance = 100m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Creates a standard account in an accounting period with its opening date.
    /// </summary>
    [Fact]
    public async Task AccountCreateAsyncPersistsOpeningPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        AccountModel created = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Savings",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });

        Assert.Equal("Savings", created.Name);
        Assert.Equal(AccountTypeModel.Standard, created.Type);
    }

    /// <summary>
    /// Rejects account dates outside the opening period and onboarding after accounting has begun.
    /// </summary>
    [Fact]
    public async Task AccountCreationAndOnboardingEnforceAccountingPeriodBoundaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage invalidDate = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Invalid date",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 10, 1)
        });
        using HttpResponseMessage lateOnboarding = await test.Api.PostResponseAsync("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Late onboarding",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 10m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidDate.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, lateOnboarding.StatusCode);
    }

    /// <summary>
    /// Adds and removes resources from every existing period history when their opening period has successors.
    /// </summary>
    [Fact]
    public async Task CreateAndDeleteAsyncMaintainResourceMembershipAcrossLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        AccountModel account = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Savings",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });
        FundModel fund = await test.Api.PostAsync<CreateFundModel, FundModel>("/funds", new CreateFundModel
        {
            Name = "Travel",
            Description = "Travel",
            AccountingPeriodId = july.Id
        });
        FundGoalModel julyGoal = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/fund/{fund.Id}?accountingPeriodId={july.Id}");
        FundGoalModel augustGoal = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/fund/{fund.Id}?accountingPeriodId={august.Id}");

        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        Assert.Contains(accounts.Accounts.Items, item => item.Id == account.Id);
        Assert.Contains(funds.Funds.Items, item => item.Id == fund.Id);
        Assert.NotEqual(julyGoal.Id, augustGoal.Id);

        await test.Api.DeleteAsync($"/accounts/{account.Id}");
        await test.Api.DeleteAsync($"/funds/{fund.Id}");
        using HttpResponseMessage missingJulyGoal = await test.Api.GetResponseAsync($"/fund-goals/fund/{fund.Id}?accountingPeriodId={july.Id}");
        using HttpResponseMessage missingAugustGoal = await test.Api.GetResponseAsync($"/fund-goals/fund/{fund.Id}?accountingPeriodId={august.Id}");

        accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        Assert.DoesNotContain(accounts.Accounts.Items, item => item.Id == account.Id);
        Assert.DoesNotContain(funds.Funds.Items, item => item.Id == fund.Id);
        Assert.Equal(HttpStatusCode.NotFound, missingJulyGoal.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingAugustGoal.StatusCode);
    }
}
