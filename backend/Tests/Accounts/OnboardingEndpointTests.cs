using System.Net;
using Models;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Covers explicit Account and Fund onboarding contracts.
/// </summary>
public sealed class OnboardingEndpointTests
{
    /// <summary>
    /// Persists onboarding balances and Fund Goal configuration before an Accounting Period exists.
    /// </summary>
    [Fact]
    public async Task OnboardEndpointsExposeBalancesAndFundGoalConfiguration()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountModel account = await test.Api.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Cash",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 120m
        });
        FundModel fund = await test.Api.PostAsync<OnboardFundModel, FundModel>("/funds/onboard", new OnboardFundModel
        {
            Name = "Emergency",
            Description = "Emergency",
            OnboardedBalance = 80m,
            PlannedMonthlyContribution = 25m,
            MinimumEndingBalance = 50m,
            MaximumEndingBalance = 100m
        });

        CollectionModel<AccountWithBalanceModel> accounts = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>("/accounts/with-balances");
        CollectionModel<FundWithBalanceModel> funds = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        FundGoalModel goal = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/fund/{fund.Id}");

        Assert.Equal(120m, Assert.Single(accounts.Items, item => item.Id == account.Id).CurrentBalance.PostedBalance);
        Assert.Equal(80m, Assert.Single(funds.Items, item => item.Id == fund.Id).CurrentBalance.PostedBalance);
        Assert.Equal(25m, goal.PlannedMonthlyContribution);
        Assert.Equal(50m, goal.MinimumEndingBalance);
        Assert.Equal(100m, goal.MaximumEndingBalance);
    }

    /// <summary>
    /// Keeps onboarding allocations within the unassigned balance and restores that balance when a Fund is deleted.
    /// </summary>
    [Fact]
    public async Task FundOnboardingAsyncTransfersAndRestoresUnassignedBalance()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Api.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Cash",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 100m
        });

        using HttpResponseMessage insufficient = await test.Api.PostResponseAsync("/funds/onboard", new OnboardFundModel
        {
            Name = "Too much",
            Description = "Too much",
            OnboardedBalance = 101m
        });
        FundModel fund = await test.Api.PostAsync<OnboardFundModel, FundModel>("/funds/onboard", new OnboardFundModel
        {
            Name = "Emergency",
            Description = "Emergency",
            OnboardedBalance = 70m
        });

        CollectionModel<FundWithBalanceModel> afterOnboarding = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, insufficient.StatusCode);
        Assert.Equal(30m, Assert.Single(afterOnboarding.Items, item => item.Name == "Unassigned").CurrentBalance.PostedBalance);

        await test.Api.DeleteAsync($"/funds/{fund.Id}");

        CollectionModel<FundWithBalanceModel> afterDeletion = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        Assert.Equal(100m, Assert.Single(afterDeletion.Items, item => item.Name == "Unassigned").CurrentBalance.PostedBalance);
    }

    /// <summary>
    /// Applies the inverse unassigned-balance effect for onboarded debt Accounts and restores it on deletion.
    /// </summary>
    [Fact]
    public async Task DebtAccountOnboardingAsyncUpdatesAndRestoresUnassignedBalance()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Api.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Cash",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 100m
        });
        AccountModel card = await test.Api.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Card",
            Type = AccountTypeModel.CreditCard,
            OnboardedBalance = 40m
        });

        CollectionModel<FundWithBalanceModel> afterOnboarding = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        Assert.Equal(60m, Assert.Single(afterOnboarding.Items, item => item.Name == "Unassigned").CurrentBalance.PostedBalance);

        await test.Api.DeleteAsync($"/accounts/{card.Id}");

        CollectionModel<FundWithBalanceModel> afterDeletion = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        Assert.Equal(100m, Assert.Single(afterDeletion.Items, item => item.Name == "Unassigned").CurrentBalance.PostedBalance);
    }
}
