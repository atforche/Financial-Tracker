using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for account transfer balance lifecycles.
/// </summary>
public sealed class AccountTransactionLifecycleTests
{
    /// <summary>
    /// Updating, independently posting, unposting, and deleting a transfer keeps every balance surface synchronized.
    /// </summary>
    [Fact]
    public async Task UpdatePostUnpostAndDeleteAsyncKeepBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle checking = await test.Accounts.Onboard("Checking").WithOpeningBalance(1000m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").WithOpeningBalance(500m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle reserve = await test.Funds.Create("Reserve").In(july).CreateAsync();
        AccountTransactionBuilder transfer = test.Transactions.Account()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From(checking)
            .To(savings);
        TransactionHandle transaction = await transfer.CreateAsync();

        await transfer.For(125m).UpdateAsync(transaction);
        await AssertBalancesAsync(test, checking, savings, july, reserve, 1000m, 875m, 500m, 625m, 1500m, 1500m, 0m, 0m, 0m, 0m);

        await test.Transactions.PostAsync(transaction, checking, new DateOnly(2026, 7, 20));
        await AssertBalancesAsync(test, checking, savings, july, reserve, 875m, 875m, 500m, 625m, 1500m, 1375m, 0m, 0m, 0m, 0m);

        await test.Transactions.PostAsync(transaction, savings, new DateOnly(2026, 7, 21));
        await AssertBalancesAsync(test, checking, savings, july, reserve, 875m, 875m, 625m, 625m, 1500m, 1500m, 0m, 0m, 0m, 0m);

        await test.Transactions.UnpostAsync(transaction);
        await AssertBalancesAsync(test, checking, savings, july, reserve, 1000m, 875m, 500m, 625m, 1500m, 1500m, 0m, 0m, 0m, 0m);

        await test.Transactions.DeleteAsync(transaction);
        await AssertBalancesAsync(test, checking, savings, july, reserve, 1000m, 1000m, 500m, 500m, 1500m, 1500m, 0m, 0m, 0m, 0m);
    }

    private static async Task AssertBalancesAsync(
        FinancialTrackerTestContext test,
        AccountHandle source,
        AccountHandle destination,
        AccountingPeriodHandle period,
        FundHandle fund,
        decimal expectedSourcePosted,
        decimal expectedSourceIncludingPending,
        decimal expectedDestinationPosted,
        decimal expectedDestinationIncludingPending,
        decimal expectedPeriodOpening,
        decimal expectedPeriodClosing,
        decimal expectedFundPosted,
        decimal expectedFundIncludingPending,
        decimal expectedGoalPosted,
        decimal expectedGoalIncludingPending)
    {
        AccountBalanceSnapshot sourceBalance = await test.AccountQueries.GetBalanceAsync(source);
        AccountBalanceSnapshot destinationBalance = await test.AccountQueries.GetBalanceAsync(destination);
        AccountingPeriodBalanceSnapshot periodBalance = await test.AccountingPeriodQueries.GetBalanceAsync(period);
        FundBalanceSnapshot fundBalance = await test.FundQueries.GetBalanceAsync(fund);
        FundGoalAvailabilitySnapshot goalAvailability = await test.FundGoalQueries.GetAvailabilityAsync(fund.Goal);

        Assert.Equal(expectedSourcePosted, sourceBalance.Posted);
        Assert.Equal(expectedSourceIncludingPending, sourceBalance.IncludingPending);
        Assert.Equal(expectedDestinationPosted, destinationBalance.Posted);
        Assert.Equal(expectedDestinationIncludingPending, destinationBalance.IncludingPending);
        Assert.Equal(expectedPeriodOpening, periodBalance.Opening);
        Assert.Equal(expectedPeriodClosing, periodBalance.Closing);
        Assert.Equal(expectedFundPosted, fundBalance.Posted);
        Assert.Equal(expectedFundIncludingPending, fundBalance.IncludingPending);
        Assert.Equal(expectedGoalPosted, goalAvailability.Posted);
        Assert.Equal(expectedGoalIncludingPending, goalAvailability.IncludingPending);
    }
}