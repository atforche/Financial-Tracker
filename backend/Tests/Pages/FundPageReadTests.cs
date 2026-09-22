using Models;
using Models.AccountingPeriods;
using Models.BalanceEvents;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Pages;

/// <summary>
/// Characterizes the current fund workspace and trends page reads.
/// </summary>
public sealed class FundPageReadTests
{
    /// <summary>
    /// Returns the fund and its accounting period choice to the workspace.
    /// </summary>
    [Fact]
    public async Task WorkspaceReturnsTheFundAndItsPeriodChoice()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?sort=DateDescending&limit=500&offset=0");
        CollectionModel<FundWithBalanceModel> funds = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>(
            "/funds/with-balances");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        Assert.Contains(funds.Items, item => item.Id == groceries.Id);
    }

    /// <summary>
    /// Preserves total fund count when the trends list is limited to one row.
    /// </summary>
    [Fact]
    public async Task TrendsReturnsTheWholeFundCountWithAOneRowList()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle travel = await test.Funds.Create("Travel").In(july).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?sort=DateDescending&limit=500&offset=0");
        FundsInDateRangeModel funds = await test.Api.GetAsync<FundsInDateRangeModel>(
            "/funds/date-range?range.start=2026-07-01&range.end=2026-07-31&limit=1&offset=0");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        Assert.Equal(3, funds.Funds.TotalCount);
        FundWithBalanceRangeModel fund = Assert.Single(funds.Funds.Items);
        Assert.True(fund.Id == groceries.Id || fund.Id == travel.Id || fund.Name == "Unassigned");
    }

    /// <summary>
    /// Keeps the fund detail's balance card, recent events, and totals in agreement.
    /// </summary>
    [Fact]
    public async Task DetailReturnsMatchingBalanceEventsAndTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 15));

        CollectionModel<FundWithBalanceModel> funds = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>(
            "/funds/with-balances");
        CollectionModel<FundBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.names=Groceries&limit=5");
        BalanceEventTotalsModel totals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            "/funds/balance-events/date-range/totals?range.start=2026-07-01&range.end=2026-07-31&filter.names=Groceries");

        Assert.Equal(40m, Assert.Single(funds.Items, item => item.Id == groceries.Id).CurrentBalance.PostedBalance);
        Assert.Contains(events.Items, item => item.Amount == 40m);
        Assert.Equal(40m, totals.TotalInflow);
    }

    /// <summary>
    /// Gives the create form a period whose balances match the selected financial state.
    /// </summary>
    [Fact]
    public async Task CreateFormReturnsPeriodBalanceChoices()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<AccountingPeriodWithBalanceModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodWithBalanceModel>>(
            "/accounting-periods/with-balances?limit=500");

        AccountingPeriodWithBalanceModel period = Assert.Single(periods.Items, item => item.Id == july.Id);
        Assert.Equal(100m, period.ClosingBalance);
    }
}
