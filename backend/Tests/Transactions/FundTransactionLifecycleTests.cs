using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for fund transfer balance lifecycles.
/// </summary>
public sealed class FundTransactionLifecycleTests
{
    /// <summary>
    /// Creating, updating, and deleting a fund transfer keeps fund and fund-goal balances synchronized without affecting accounts.
    /// </summary>
    [Fact]
    public async Task CreateUpdateAndDeleteAsyncKeepBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        FundTransactionBuilder transfer = test.Transactions.Fund()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From(groceries)
            .To(dining);
        TransactionHandle transaction = await transfer.CreateAsync();

        await AssertBalancesAsync(test, cash, july, groceries, dining, 1000m, 1000m, -80m, -80m, 80m, 80m, -80m, -80m, 80m, 80m);

        await transfer.For(125m).UpdateAsync(transaction);
        await AssertBalancesAsync(test, cash, july, groceries, dining, 1000m, 1000m, -125m, -125m, 125m, 125m, -125m, -125m, 125m, 125m);

        await test.Transactions.DeleteAsync(transaction);
        await AssertBalancesAsync(test, cash, july, groceries, dining, 1000m, 1000m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
    }

    private static async Task AssertBalancesAsync(
        FinancialTrackerTestContext test,
        AccountHandle account,
        AccountingPeriodHandle period,
        FundHandle source,
        FundHandle destination,
        decimal expectedAccountPosted,
        decimal expectedPeriodClosing,
        decimal expectedSourcePosted,
        decimal expectedSourceIncludingPending,
        decimal expectedDestinationPosted,
        decimal expectedDestinationIncludingPending,
        decimal expectedSourceGoalPosted,
        decimal expectedSourceGoalIncludingPending,
        decimal expectedDestinationGoalPosted,
        decimal expectedDestinationGoalIncludingPending)
    {
        AccountBalanceSnapshot accountBalance = await test.AccountQueries.GetBalanceAsync(account);
        AccountingPeriodBalanceSnapshot periodBalance = await test.AccountingPeriodQueries.GetBalanceAsync(period);
        FundBalanceSnapshot sourceBalance = await test.FundQueries.GetBalanceAsync(source);
        FundBalanceSnapshot destinationBalance = await test.FundQueries.GetBalanceAsync(destination);
        FundGoalAvailabilitySnapshot sourceGoal = await test.FundGoalQueries.GetAvailabilityAsync(source.Goal);
        FundGoalAvailabilitySnapshot destinationGoal = await test.FundGoalQueries.GetAvailabilityAsync(destination.Goal);

        Assert.Equal(expectedAccountPosted, accountBalance.Posted);
        Assert.Equal(expectedAccountPosted, accountBalance.IncludingPending);
        Assert.Equal(expectedAccountPosted, periodBalance.Opening);
        Assert.Equal(expectedPeriodClosing, periodBalance.Closing);
        Assert.Equal(expectedSourcePosted, sourceBalance.Posted);
        Assert.Equal(expectedSourceIncludingPending, sourceBalance.IncludingPending);
        Assert.Equal(expectedDestinationPosted, destinationBalance.Posted);
        Assert.Equal(expectedDestinationIncludingPending, destinationBalance.IncludingPending);
        Assert.Equal(expectedSourceGoalPosted, sourceGoal.Posted);
        Assert.Equal(expectedSourceGoalIncludingPending, sourceGoal.IncludingPending);
        Assert.Equal(expectedDestinationGoalPosted, destinationGoal.Posted);
        Assert.Equal(expectedDestinationGoalIncludingPending, destinationGoal.IncludingPending);
    }
}
