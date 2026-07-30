using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Infrastructure;

namespace Tests.Queries;

/// <summary>
/// Covers sort mappings that are selected by REST query parameters.
/// </summary>
public sealed class SortContractCoverageTests
{
    /// <summary>
    /// Sorts resource and range lists by values other than their default names.
    /// </summary>
    [Fact]
    public async Task ListsAndRangesHonorNonDefaultSorts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle card = await test.Accounts.Onboard("Card").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        _ = await test.Funds.Create("Alpha").In(july).CreateAsync();
        _ = await test.Funds.Create("Zulu").In(july).CreateAsync();

        CollectionModel<AccountModel> accounts = await test.Api.GetAsync<CollectionModel<AccountModel>>("/accounts?sort=TypeDescending");
        CollectionModel<FundModel> funds = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds?sort=DescriptionDescending");
        AccountingPeriodsInRangeModel periods = await test.Api.GetAsync<AccountingPeriodsInRangeModel>(
            $"/accounting-periods/range?range.start={july.Id}&range.end={august.Id}&sort=Date");
        AccountsInAccountingPeriodRangeModel accountRange = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}&sort=EndingBalanceDescending");

        Assert.Equal(2, accounts.TotalCount);
        Assert.Contains(accounts.Items, account => account.Id == cash.Id);
        Assert.Contains(accounts.Items, account => account.Id == card.Id);
        Assert.Equal(["Zulu", "Alpha"], funds.Items.Where(fund => fund.Name != "Unassigned").Select(fund => fund.Name));
        Assert.Equal([july.Id, august.Id], periods.AccountingPeriods.Items.Select(period => period.Id));
        Assert.Equal(cash.Id, accountRange.Accounts.Items.First().Id);
    }
}