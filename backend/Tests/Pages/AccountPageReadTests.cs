using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.BalanceEvents;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Pages;

/// <summary>
/// Characterizes the current account workspace and trends page reads.
/// </summary>
public sealed class AccountPageReadTests
{
    /// <summary>
    /// Keeps period choices, account balances, and institution choices available together.
    /// </summary>
    [Fact]
    public async Task WorkspaceReturnsTheAccountAndItsFilterChoices()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithFinancialInstitution("Bank").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?sort=DateDescending&limit=500&offset=0");
        CollectionModel<AccountWithBalanceModel> accounts = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>(
            "/accounts/with-balances");
        CollectionModel<string> institutions = await test.Api.GetAsync<CollectionModel<string>>(
            "/accounts/financial-institutions");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        AccountWithBalanceModel account = Assert.Single(accounts.Items, item => item.Id == cash.Id);
        Assert.Equal(100m, account.CurrentBalance.PostedBalance);
        Assert.Contains("Bank", institutions.Items);
    }

    /// <summary>
    /// Gives trends an unpaged total even when its account list is limited.
    /// </summary>
    [Fact]
    public async Task TrendsReturnsTheWholeBalanceSummaryWithAOneRowList()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        _ = await test.Accounts.Onboard("Savings").WithOpeningBalance(50m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?sort=DateDescending&limit=500&offset=0");
        AccountsInDateRangeModel accounts = await test.Api.GetAsync<AccountsInDateRangeModel>(
            "/accounts/date-range?range.start=2026-07-01&range.end=2026-07-31&limit=1&offset=0");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        Assert.Equal(2, accounts.Accounts.TotalCount);
        AccountWithBalanceRangeModel account = Assert.Single(accounts.Accounts.Items);
        Assert.True(account.Id == cash.Id || account.StartingBalance == 50m);
    }

    /// <summary>
    /// Keeps the account detail's balance card, recent events, and totals in agreement.
    /// </summary>
    [Fact]
    public async Task DetailReturnsMatchingBalanceEventsAndTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 15));

        CollectionModel<AccountWithBalanceModel> accounts = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>(
            "/accounts/with-balances");
        CollectionModel<AccountBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?limit=5");
        BalanceEventTotalsModel totals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            $"/accounts/{cash.Id}/balance-events/totals");

        Assert.Equal(80m, Assert.Single(accounts.Items, item => item.Id == cash.Id).CurrentBalance.PostedBalance);
        Assert.Contains(events.Items, item => item.Amount == 20m);
        Assert.Equal(20m, totals.TotalOutflow);
    }
}
