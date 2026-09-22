using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Pages;

/// <summary>
/// Characterizes the current backend reads used to render the overview page.
/// </summary>
public sealed class OverviewPageReadTests
{
    /// <summary>
    /// Gives the page matching period options and account and fund totals for one date range.
    /// </summary>
    [Fact]
    public async Task DateRangeReturnsMatchingPeriodOptionsAndPostedBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 15));

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?sort=DateDescending&limit=500&offset=0");
        AccountsInDateRangeModel accounts = await test.Api.GetAsync<AccountsInDateRangeModel>(
            "/accounts/date-range?range.start=2026-07-01&range.end=2026-07-31&limit=1&offset=0");
        FundsInDateRangeModel funds = await test.Api.GetAsync<FundsInDateRangeModel>(
            "/funds/date-range?range.start=2026-07-01&range.end=2026-07-31&limit=1&offset=0");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        Assert.Equal(40m, accounts.TotalIncome.Total);
        Assert.Equal(40m, funds.TotalIncome.Tracked);
        Assert.Contains(accounts.Accounts.Items, item => item.Id == cash.Id);
        Assert.Contains(funds.Funds.Items, item => item.Id == groceries.Id);
    }
}
