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
            RegularContribution = 25m,
            MinimumFundedBalance = 50m,
            MaximumFundedBalance = 100m,
            TargetEndingBalance = 75m
        });

        CollectionModel<AccountWithBalanceModel> accounts = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>("/accounts/with-balances");
        CollectionModel<FundWithBalanceModel> funds = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        FundGoalModel goal = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/fund/{fund.Id}");

        Assert.Equal(120m, Assert.Single(accounts.Items, item => item.Id == account.Id).CurrentBalance.PostedBalance);
        Assert.Equal(80m, Assert.Single(funds.Items, item => item.Id == fund.Id).CurrentBalance.PostedBalance);
        Assert.Equal(25m, goal.RegularContribution);
        Assert.Equal(50m, goal.MinimumFundedBalance);
        Assert.Equal(100m, goal.MaximumFundedBalance);
        Assert.Equal(75m, goal.TargetEndingBalance);
    }
}
