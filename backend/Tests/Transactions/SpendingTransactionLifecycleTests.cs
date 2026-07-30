using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for spending transaction balance lifecycles.
/// </summary>
public sealed class SpendingTransactionLifecycleTests
{
    /// <summary>
    /// Updating, posting, unposting, and deleting spending keeps each balance surface synchronized.
    /// </summary>
    [Fact]
    public async Task UpdatePostUnpostAndDeleteAsyncKeepBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        SpendingTransactionBuilder spending = test.Transactions.Spending()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From(cash)
            .To("Market", groceries);
        TransactionHandle transaction = await spending.CreateAsync();

        await spending.For(125m).UpdateAsync(transaction);
        await AssertBalancesAsync(test, cash, july, groceries, 1000m, 875m, 0m, -125m, 1000m, 1000m, 0m, -125m);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 20));
        await AssertBalancesAsync(test, cash, july, groceries, 875m, 875m, -125m, -125m, 1000m, 875m, -125m, -125m);

        await test.Transactions.UnpostAsync(transaction);
        await AssertBalancesAsync(test, cash, july, groceries, 1000m, 875m, 0m, -125m, 1000m, 1000m, 0m, -125m);

        await test.Transactions.DeleteAsync(transaction);
        await AssertBalancesAsync(test, cash, july, groceries, 1000m, 1000m, 0m, 0m, 1000m, 1000m, 0m, 0m);
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