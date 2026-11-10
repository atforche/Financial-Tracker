using Models.Accounts;
using Models.Funds;

namespace Tests.Ranges;

/// <summary>
/// Covers the account and fund summaries returned for accounting-period and date ranges.
/// </summary>
public sealed class RangeSummaryTests
{
    /// <summary>
    /// Returns balance boundaries across contiguous periods.
    /// </summary>
    [Fact]
    public async Task RangesReturnBalanceBoundaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 8, 15)).For(15m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 8, 15));

        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        AccountWithBalanceRangeModel account = Assert.Single(accounts.Accounts.Items, item => item.Id == cash.Id);
        FundWithBalanceRangeModel fund = Assert.Single(funds.Funds.Items, item => item.Id == groceries.Id);
        Assert.Equal(100m, account.StartingBalance);
        Assert.Equal(125m, account.EndingBalance);
        Assert.Equal(0m, fund.StartingBalance);
        Assert.Equal(25m, fund.EndingBalance);
        Assert.Equal(2, accounts.AccountingPeriods.Count);
        Assert.Equal(2, funds.AccountingPeriods.Count);
    }

    /// <summary>
    /// Omits accounts before their opening date and reports debt and unassigned-fund totals correctly.
    /// </summary>
    [Fact]
    public async Task DateRangeSummariesRespectOpeningDatesDebtSignsAndUnassignedFunds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await test.Accounts.Create("Card", july).WithType(AccountTypeModel.CreditCard).OpenedOn(new DateOnly(2026, 7, 15)).CreateAsync();
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
    }

    /// <summary>
    /// Reports opening balances from before the start date so activity on the start date counts as range change.
    /// </summary>
    [Fact]
    public async Task DateRangeOpeningBalancesExcludeStartDateActivity()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));

        AccountsInDateRangeModel accounts = await test.Api.GetAsync<AccountsInDateRangeModel>(
            "/accounts/date-range?range.start=2026-07-10&range.end=2026-07-31");
        FundsInDateRangeModel funds = await test.Api.GetAsync<FundsInDateRangeModel>(
            "/funds/date-range?range.start=2026-07-10&range.end=2026-07-31");

        Assert.Equal(100m, accounts.OpeningBalance?.TotalBalance);
        Assert.Equal(140m, accounts.Dates.First().TotalBalance);
        Assert.Equal(new DateOnly(2026, 7, 10), accounts.Dates.First().Date);
        Assert.Equal(0m, funds.OpeningBalance?.TotalAssignedBalance);
        Assert.Equal(40m, funds.Dates.First().TotalAssignedBalance);
    }

    /// <summary>
    /// Sorts, filters, and pages the accounts and funds in an accounting-period range.
    /// </summary>
    [Fact]
    public async Task AccountingPeriodRangesSortFilterAndPage()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        _ = await test.Funds.Create("Alpha").In(july).CreateAsync();
        FundHandle beta = await test.Funds.Create("Beta").In(july).CreateAsync();

        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}&sort=EndingBalanceDescending&limit=1");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}&filter.names=Alpha&filter.names=Beta&sort=NameDescending&limit=1");

        Assert.Equal(1, accounts.Accounts.TotalCount);
        Assert.Equal(cash.Id, Assert.Single(accounts.Accounts.Items).Id);
        Assert.Equal(2, funds.Funds.TotalCount);
        Assert.Equal(beta.Id, Assert.Single(funds.Funds.Items).Id);
    }
}
