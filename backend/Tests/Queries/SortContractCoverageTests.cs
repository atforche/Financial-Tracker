using Models.AccountingPeriods;
using Models.Accounts;
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
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        AccountingPeriodsInRangeModel periods = await test.Api.GetAsync<AccountingPeriodsInRangeModel>(
            $"/accounting-periods/range?range.start={july.Id}&range.end={august.Id}&sort=Date");
        AccountsInAccountingPeriodRangeModel accountRange = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}&sort=EndingBalanceDescending");

        Assert.Equal([july.Id, august.Id], periods.AccountingPeriods.Items.Select(period => period.Id));
        Assert.Equal(cash.Id, accountRange.Accounts.Items.First().Id);
    }
}
