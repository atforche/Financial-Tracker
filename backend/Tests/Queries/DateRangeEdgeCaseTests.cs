using Models.Accounts;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Infrastructure;

namespace Tests.Queries;

/// <summary>
/// Covers date-range summaries with account-type, opening-date, and unassigned-fund boundaries.
/// </summary>
public sealed class DateRangeEdgeCaseTests
{
    /// <summary>
    /// Omits accounts before their opening date and reports debt and unassigned-fund totals correctly.
    /// </summary>
    [Fact]
    public async Task DateRangeSummariesRespectOpeningDatesDebtSignsAndUnassignedFunds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountModel card = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Card",
            Type = AccountTypeModel.CreditCard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 15)
        });
        _ = await test.Funds.Create("Groceries").In(july).CreateAsync();

        AccountsInDateRangeModel accounts = await test.Api.GetAsync<AccountsInDateRangeModel>(
            "/accounts/date-range?range.start=2026-07-01&range.end=2026-07-31");
        FundsInDateRangeModel funds = await test.Api.GetAsync<FundsInDateRangeModel>(
            "/funds/date-range?range.start=2026-07-01&range.end=2026-07-31");

        AccountBalanceSummaryByDateModel beforeCardOpens = Assert.Single(accounts.Dates, item => item.Date == new DateOnly(2026, 7, 14));
        AccountBalanceSummaryByDateModel afterCardOpens = Assert.Single(accounts.Dates, item => item.Date == new DateOnly(2026, 7, 15));
        Assert.Equal(100m, beforeCardOpens.TotalBalance);
        Assert.Equal(100m, afterCardOpens.TotalBalance);
        Assert.Equal(100m, afterCardOpens.TotalTrackedBalance);
        Assert.Contains(accounts.Accounts.Items, item => item.Id == card.Id);
        Assert.Equal(0m, funds.Dates.First().TotalAssignedBalance);
        Assert.Equal(100m, funds.Dates.First().TotalUnassignedBalance);
        Assert.NotEqual(Guid.Empty, cash.Id);
    }
}