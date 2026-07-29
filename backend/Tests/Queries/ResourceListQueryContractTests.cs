using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Queries;

/// <summary>
/// Covers filtering, ordering, paging, and total counts for non-transaction resource lists.
/// </summary>
public sealed class ResourceListQueryContractTests
{
    /// <summary>
    /// Applies resource filters and preserves totals when account and fund lists are paged.
    /// </summary>
    [Fact]
    public async Task AccountAndFundListsFilterSortAndPage()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Beta cash").CreateAsync();
        _ = await test.Accounts.Onboard("Alpha cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        _ = await test.Funds.Create("Beta fund").In(july).CreateAsync();
        _ = await test.Funds.Create("Alpha fund").In(july).CreateAsync();

        CollectionModel<AccountModel> accounts = await test.Api.GetAsync<CollectionModel<AccountModel>>(
            "/accounts?filter.nameSearch=cash&sort=Name&limit=1");
        CollectionModel<FundModel> funds = await test.Api.GetAsync<CollectionModel<FundModel>>(
            "/funds?filter.nameSearch=fund&sort=NameDescending&limit=1");

        Assert.Equal(2, accounts.TotalCount);
        Assert.Equal("Alpha cash", Assert.Single(accounts.Items).Name);
        Assert.Equal(2, funds.TotalCount);
        Assert.Equal("Beta fund", Assert.Single(funds.Items).Name);
    }

    /// <summary>
    /// Orders range balances and filters Accounting Period and Fund Goal resource lists.
    /// </summary>
    [Fact]
    public async Task RangeAndRelatedResourceListsHonorTheirQueryContracts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle alpha = await test.Funds.Create("Alpha").In(july).CreateAsync();
        FundHandle beta = await test.Funds.Create("Beta").In(july).CreateAsync();

        AccountsInAccountingPeriodRangeModel accountRange = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}&sort=EndingBalanceDescending&limit=1");
        FundsInAccountingPeriodRangeModel fundRange = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}&filter.names=Alpha&filter.names=Beta&sort=NameDescending&limit=1");
        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?filter.months=8&sort=DateDescending");
        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.fundIds={alpha.Id}&sort=Fund");

        Assert.Equal(1, accountRange.Accounts.TotalCount);
        Assert.Equal(cash.Id, Assert.Single(accountRange.Accounts.Items).Id);
        Assert.Equal(2, fundRange.Funds.TotalCount);
        Assert.Equal(beta.Id, Assert.Single(fundRange.Funds.Items).Id);
        Assert.Equal(august.Id, Assert.Single(periods.Items).Id);
        Assert.Equal(2, goals.TotalCount);
        Assert.All(goals.Items, goal => Assert.Equal(alpha.Id, goal.Fund.Id));
        Assert.Contains(goals.Items, goal => goal.Id == alpha.Goal.Id);
    }
}