using Models.Accounts;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers propagation of posted transaction effects into later Accounting Period snapshots.
/// </summary>
public sealed class CrossPeriodBalancePropagationTests
{
    /// <summary>
    /// Posting, unposting, and reposting an earlier income updates later Account and Fund boundaries.
    /// </summary>
    [Fact]
    public async Task EarlierTransactionLifecycleUpdatesLaterPeriodBoundaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        IncomeTransactionBuilder transaction = test.Transactions.Income()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(40m)
            .From("Employer")
            .To(cash, income);
        TransactionHandle handle = await transaction.CreateAsync();

        await test.Transactions.PostAsync(handle, cash, new DateOnly(2026, 7, 15));
        await AssertAugustBoundariesAsync(test, july, august, cash, income, 140m, 40m);

        await test.Transactions.UnpostAsync(handle);
        await AssertAugustBoundariesAsync(test, july, august, cash, income, 100m, 0m);

        await transaction.For(65m).UpdateAsync(handle);
        await test.Transactions.PostAsync(handle, cash, new DateOnly(2026, 7, 15));
        await AssertAugustBoundariesAsync(test, july, august, cash, income, 165m, 65m);
    }

    private static async Task AssertAugustBoundariesAsync(
        FinancialTrackerTestContext test,
        AccountingPeriodHandle july,
        AccountingPeriodHandle august,
        AccountHandle account,
        FundHandle fund,
        decimal expectedAccountBalance,
        decimal expectedFundBalance)
    {
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        Assert.Equal(expectedAccountBalance, Assert.Single(accounts.Accounts.Items, item => item.Id == account.Id).EndingBalance);
        Assert.Equal(expectedFundBalance, Assert.Single(funds.Funds.Items, item => item.Id == fund.Id).EndingBalance);
        Assert.Equal(expectedAccountBalance, Assert.Single(accounts.AccountingPeriods, item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalBalance);
        Assert.Equal(expectedFundBalance, Assert.Single(funds.AccountingPeriods, item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalAssignedBalance);
    }
}
