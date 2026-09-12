using Models.BalanceEvents;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>Verifies persisted daily balances isolate movements by Accounting Period.</summary>
public sealed class AccountingPeriodBalanceDateTests
{
    /// <summary>Interleaved posted dates from different periods do not leak into either chart.</summary>
    [Fact]
    public async Task DailyBalancesUseOnlyTheirAccountingPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle reserve = await test.Funds.Create("Reserve").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        TransactionHandle julyIncome = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10))
            .For(40m).From("Employer").To(cash, reserve).CreateAsync();
        TransactionHandle augustIncome = await test.Transactions.Income().In(august).On(new DateOnly(2026, 7, 15))
            .For(20m).From("Employer").To(cash, reserve).CreateAsync();
        TransactionHandle julySpending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20))
            .For(10m).From(cash).To("Purchase", reserve).CreateAsync();
        await test.Transactions.PostAsync(julyIncome, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(augustIncome, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(julySpending, cash, new DateOnly(2026, 7, 20));

        PeriodBalanceDateRangeModel accountJuly = await GetAccountDatesAsync(test, cash, july);
        PeriodBalanceDateRangeModel accountAugust = await GetAccountDatesAsync(test, cash, august);
        PeriodBalanceDateRangeModel fundJuly = await GetFundDatesAsync(test, reserve, july);
        PeriodBalanceDateRangeModel fundAugust = await GetFundDatesAsync(test, reserve, august);

        Assert.Equal(31, accountJuly.Dates.Count);
        Assert.Equal(48, accountAugust.Dates.Count);
        Assert.Equal(100m, accountJuly.OpeningBalance);
        Assert.Equal(100m, BalanceOn(accountJuly, 7, 9));
        Assert.Equal(140m, BalanceOn(accountJuly, 7, 10));
        Assert.Equal(140m, BalanceOn(accountJuly, 7, 15));
        Assert.Equal(130m, BalanceOn(accountJuly, 7, 20));
        Assert.Equal(130m, accountAugust.OpeningBalance);
        Assert.Equal(150m, BalanceOn(accountAugust, 8, 1));
        Assert.Equal(0m, fundJuly.OpeningBalance);
        Assert.Equal(40m, BalanceOn(fundJuly, 7, 15));
        Assert.Equal(30m, BalanceOn(fundJuly, 7, 20));
        Assert.Equal(30m, fundAugust.OpeningBalance);
        Assert.Equal(50m, BalanceOn(fundAugust, 8, 1));

        await test.Transactions.UnpostAsync(julySpending);
        accountJuly = await GetAccountDatesAsync(test, cash, july);
        accountAugust = await GetAccountDatesAsync(test, cash, august);
        fundJuly = await GetFundDatesAsync(test, reserve, july);
        fundAugust = await GetFundDatesAsync(test, reserve, august);
        Assert.Equal(140m, BalanceOn(accountJuly, 7, 20));
        Assert.Equal(140m, accountAugust.OpeningBalance);
        Assert.Equal(160m, BalanceOn(accountAugust, 8, 1));
        Assert.Equal(40m, BalanceOn(fundJuly, 7, 20));
        Assert.Equal(40m, fundAugust.OpeningBalance);
        Assert.Equal(60m, BalanceOn(fundAugust, 8, 1));

        await test.Transactions.PostAsync(julySpending, cash, new DateOnly(2026, 7, 20));
        Assert.Equal(130m, BalanceOn(await GetAccountDatesAsync(test, cash, july), 7, 20));
        Assert.Equal(50m, BalanceOn(await GetFundDatesAsync(test, reserve, august), 8, 1));

        TransactionHandle lateJulyIncome = await test.Transactions.Income().In(july).On(new DateOnly(2026, 8, 5))
            .For(5m).From("Employer").To(cash, reserve).CreateAsync();
        await test.Transactions.PostAsync(lateJulyIncome, cash, new DateOnly(2026, 8, 5));
        Assert.Equal(130m, BalanceOn(await GetAccountDatesAsync(test, cash, july), 7, 31));
        Assert.Equal(155m, BalanceOn(await GetAccountDatesAsync(test, cash, august), 8, 5));
        Assert.Equal(30m, BalanceOn(await GetFundDatesAsync(test, reserve, july), 7, 31));
        Assert.Equal(55m, BalanceOn(await GetFundDatesAsync(test, reserve, august), 8, 5));
    }

    private static decimal BalanceOn(PeriodBalanceDateRangeModel range, int month, int day) =>
        Assert.Single(range.Dates, item => item.Date == new DateOnly(2026, month, day)).TotalBalance;

    private static Task<PeriodBalanceDateRangeModel> GetAccountDatesAsync(
        FinancialTrackerTestContext test, AccountHandle account, AccountingPeriodHandle period) =>
        test.Api.GetAsync<PeriodBalanceDateRangeModel>($"/accounts/{account.Id}/accounting-periods/{period.Id}/balance-dates");

    private static Task<PeriodBalanceDateRangeModel> GetFundDatesAsync(
        FinancialTrackerTestContext test, FundHandle fund, AccountingPeriodHandle period) =>
        test.Api.GetAsync<PeriodBalanceDateRangeModel>($"/funds/{fund.Id}/accounting-periods/{period.Id}/balance-dates");
}
