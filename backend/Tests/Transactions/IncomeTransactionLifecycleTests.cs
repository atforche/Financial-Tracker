using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for income transaction balance lifecycles.
/// </summary>
public sealed class IncomeTransactionLifecycleTests
{
    /// <summary>
    /// Updating, posting, unposting, and deleting income keeps each balance surface synchronized.
    /// </summary>
    [Fact]
    public async Task UpdatePostUnpostAndDeleteAsyncKeepBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        IncomeTransactionBuilder transactionBuilder = test.Transactions.Income()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From("Employer")
            .To(cash, income);
        TransactionHandle transaction = await transactionBuilder.CreateAsync();

        await transactionBuilder.For(125m).UpdateAsync(transaction);
        await AssertBalancesAsync(test, cash, july, income, 1000m, 1125m, 0m, 125m, 1000m, 1000m, 0m, 125m);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 20));
        await AssertBalancesAsync(test, cash, july, income, 1125m, 1125m, 125m, 125m, 1000m, 1125m, 125m, 125m);

        await test.Transactions.UnpostAsync(transaction);
        await AssertBalancesAsync(test, cash, july, income, 1000m, 1125m, 0m, 125m, 1000m, 1000m, 0m, 125m);

        await test.Transactions.DeleteAsync(transaction);
        await AssertBalancesAsync(test, cash, july, income, 1000m, 1000m, 0m, 0m, 1000m, 1000m, 0m, 0m);
    }

    private static async Task AssertBalancesAsync(
        FinancialTrackerTestContext test,
        AccountHandle account,
        AccountingPeriodHandle period,
        FundHandle fund,
        decimal expectedAccountPosted,
        decimal expectedAccountIncludingPending,
        decimal expectedFundPosted,
        decimal expectedFundIncludingPending,
        decimal expectedPeriodOpening,
        decimal expectedPeriodClosing,
        decimal expectedGoalPosted,
        decimal expectedGoalIncludingPending)
    {
        AccountBalanceSnapshot accountBalance = await test.AccountQueries.GetBalanceAsync(account);
        FundBalanceSnapshot fundBalance = await test.FundQueries.GetBalanceAsync(fund);
        AccountingPeriodBalanceSnapshot periodBalance = await test.AccountingPeriodQueries.GetBalanceAsync(period);
        FundGoalAvailabilitySnapshot goalAvailability = await test.FundGoalQueries.GetAvailabilityAsync(fund.Goal);

        Assert.Equal(expectedAccountPosted, accountBalance.Posted);
        Assert.Equal(expectedAccountIncludingPending, accountBalance.IncludingPending);
        Assert.Equal(expectedFundPosted, fundBalance.Posted);
        Assert.Equal(expectedFundIncludingPending, fundBalance.IncludingPending);
        Assert.Equal(expectedPeriodOpening, periodBalance.Opening);
        Assert.Equal(expectedPeriodClosing, periodBalance.Closing);
        Assert.Equal(expectedGoalPosted, goalAvailability.Posted);
        Assert.Equal(expectedGoalIncludingPending, goalAvailability.IncludingPending);
    }
}