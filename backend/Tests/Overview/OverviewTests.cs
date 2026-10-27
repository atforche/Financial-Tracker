using System.Net;
using Models.Accounts;
using Models.Funds;
using Models.Overview;

namespace Tests.Overview;

/// <summary>
/// Covers the Account and Fund balance summaries returned for the overview.
/// </summary>
public sealed class OverviewTests
{
    /// <summary>
    /// Matches the Account and Fund date-range summaries, counting activity on the start date as range change.
    /// </summary>
    [Fact]
    public async Task DateRangeMatchesAccountAndFundSummaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await test.Accounts.Create("Card", july).WithType(AccountTypeModel.CreditCard).OpenedOn(new DateOnly(2026, 7, 15)).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(15m).From(card).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(spending, card, new DateOnly(2026, 7, 20));

        const string range = "range.start=2026-07-10&range.end=2026-07-31";
        OverviewDateRangeModel overview = await test.Api.GetAsync<OverviewDateRangeModel>($"/overview/date-range?{range}");
        AccountsInDateRangeModel accounts = await test.Api.GetAsync<AccountsInDateRangeModel>($"/accounts/date-range?{range}");
        FundsInDateRangeModel funds = await test.Api.GetAsync<FundsInDateRangeModel>($"/funds/date-range?{range}");

        Assert.Equal(100m, overview.AccountOpeningBalance.TotalBalance);
        Assert.Equal(125m, overview.AccountClosingBalance.TotalBalance);
        Assert.Equal(-15m, Assert.Single(overview.AccountClosingBalance.BalanceByAccountType,
            balance => balance.AccountType == AccountTypeModel.CreditCard).TotalBalance);
        Assert.Equal(0m, overview.FundOpeningBalance.TotalAssignedBalance);
        Assert.Equal(25m, overview.FundClosingBalance.TotalAssignedBalance);
        Assert.Equal(100m, overview.FundClosingBalance.TotalUnassignedBalance);
        Assert.Equal(new DateOnly(2026, 7, 10), overview.AccountBalances.First().Date);
        Assert.Equal(140m, overview.AccountBalances.First().TotalBalance);

        Assert.Equivalent(accounts.OpeningBalance, overview.AccountOpeningBalance, strict: true);
        Assert.Equal(accounts.Dates.Select(item => (item.Date, item.TotalBalance)),
            overview.AccountBalances.Select(item => (item.Date, item.TotalBalance)));
        Assert.Equivalent(funds.OpeningBalance, overview.FundOpeningBalance, strict: true);
        Assert.Equal(funds.Dates.Select(item => (item.Date, item.TotalBalance)),
            overview.FundBalances.Select(item => (item.Date, item.TotalBalance)));
    }

    /// <summary>
    /// Matches the Account and Fund Accounting Period range summaries.
    /// </summary>
    [Fact]
    public async Task AccountingPeriodRangeMatchesAccountAndFundSummaries()
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

        string range = $"range.start={july.Id}&range.end={august.Id}";
        OverviewAccountingPeriodRangeModel overview = await test.Api.GetAsync<OverviewAccountingPeriodRangeModel>($"/overview/accounting-period-range?{range}");
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>($"/accounts/accounting-period-range?{range}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>($"/funds/accounting-period-range?{range}");

        Assert.Equal(100m, overview.AccountOpeningBalance.TotalBalance);
        Assert.Equal(125m, overview.AccountClosingBalance.TotalBalance);
        Assert.Equal(25m, overview.FundClosingBalance.TotalAssignedBalance);
        Assert.Equal([july.Id, august.Id], overview.AccountBalances.Select(item => item.AccountingPeriod.Id));

        Assert.Equivalent(accounts.AccountingPeriods.First().OpeningBalance, overview.AccountOpeningBalance, strict: true);
        Assert.Equivalent(accounts.AccountingPeriods.Last().ClosingBalance, overview.AccountClosingBalance, strict: true);
        Assert.Equal(accounts.AccountingPeriods.Select(item => (item.OpeningBalance.TotalBalance, item.ClosingBalance.TotalBalance)),
            overview.AccountBalances.Select(item => (item.OpeningBalance, item.ClosingBalance)));
        Assert.Equivalent(funds.AccountingPeriods.First().OpeningBalance, overview.FundOpeningBalance, strict: true);
        Assert.Equivalent(funds.AccountingPeriods.Last().ClosingBalance, overview.FundClosingBalance, strict: true);
        Assert.Equal(funds.AccountingPeriods.Select(item => (item.OpeningBalance.TotalBalance, item.ClosingBalance.TotalBalance)),
            overview.FundBalances.Select(item => (item.OpeningBalance, item.ClosingBalance)));
    }

    /// <summary>
    /// Rejects incomplete, reversed, and unknown ranges.
    /// </summary>
    [Fact]
    public async Task InvalidRangesAreRejected()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage missingEnd = await test.Api.GetResponseAsync("/overview/date-range?range.start=2026-07-01");
        using HttpResponseMessage reversed = await test.Api.GetResponseAsync("/overview/date-range?range.start=2026-07-31&range.end=2026-07-01");
        using HttpResponseMessage unknownPeriod = await test.Api.GetResponseAsync(
            $"/overview/accounting-period-range?range.start={Guid.NewGuid()}&range.end={july.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingEnd.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reversed.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownPeriod.StatusCode);
    }
}
