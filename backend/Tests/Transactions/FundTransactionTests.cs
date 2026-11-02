using Models.AccountingPeriods;
using Models.FundGoals;

namespace Tests.Transactions;

/// <summary>
/// Covers how fund transfers affect fund, fund-goal, and account balances.
/// </summary>
public sealed class FundTransactionTests
{
    /// <summary>
    /// Creating, updating, and deleting a fund transfer keeps fund balances synchronized without affecting accounts.
    /// </summary>
    [Fact]
    public async Task CreateUpdateAndDeleteAsyncKeepBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        FundTransactionBuilder transfer = test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 15)).For(80m).From(groceries).To(dining);
        TransactionHandle transaction = await transfer.CreateAsync();

        await test.AssertBalanceAsync(groceries, -80m, -80m);
        await test.AssertBalanceAsync(dining, 80m, 80m);

        await transfer.For(125m).UpdateAsync(transaction);
        await test.AssertBalanceAsync(groceries, -125m, -125m);
        await test.AssertBalanceAsync(dining, 125m, 125m);
        await test.AssertBalanceAsync(cash, 1000m, 1000m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(groceries, 0m, 0m);
        await test.AssertBalanceAsync(dining, 0m, 0m);
        await test.AssertBalanceAsync(cash, 1000m, 1000m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);
    }

    /// <summary>
    /// Updating an earlier fund transfer replays later fund balances without changing accounts or periods.
    /// </summary>
    [Fact]
    public async Task UpdateEarlierTransferReplaysLaterFundBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        FundTransactionBuilder earlier = test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 15)).For(80m).From(groceries).To(dining);
        TransactionHandle earlierTransaction = await earlier.CreateAsync();
        _ = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 16)).For(120m).From(groceries).To(dining).CreateAsync();

        await earlier.For(100m).UpdateAsync(earlierTransaction);

        await test.AssertBalanceAsync(cash, 1000m, 1000m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);
        await test.AssertBalanceAsync(groceries, -220m, -220m);
        await test.AssertBalanceAsync(dining, 220m, 220m);
    }

    /// <summary>
    /// Every destination of a split fund transfer receives and reverses its effect without counting as a goal contribution.
    /// </summary>
    [Fact]
    public async Task SplitTransferUpdatesEveryDestinationThenReversesOnDelete()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle source = await test.Funds.Create("Source").In(july).CreateAsync();
        FundHandle first = await test.Funds.Create("First").In(july).CreateAsync();
        FundHandle second = await test.Funds.Create("Second").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 15)).For(50m)
            .From(source).To(first, 20m).To(second, 30m).CreateAsync();

        await test.AssertBalanceAsync(source, -50m, -50m);
        await test.AssertBalanceAsync(first, 20m, 20m);
        await test.AssertBalanceAsync(second, 30m, 30m);
        Assert.Equal(0m, (await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}")).ActualGoalContributions);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(source, 0m, 0m);
        await test.AssertBalanceAsync(first, 0m, 0m);
        await test.AssertBalanceAsync(second, 0m, 0m);
        Assert.Equal(0m, (await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}")).ActualGoalContributions);
    }

    /// <summary>
    /// Changing a transfer's funds removes every effect from the old funds and their goals.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncMovesEffectsToNewFundsAndGoals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle oldSource = await test.Funds.Create("Old source").In(july).CreateAsync();
        FundHandle oldDestination = await test.Funds.Create("Old destination").In(july).CreateAsync();
        FundHandle newSource = await test.FundQueries.GetUnassignedAsync(july);
        FundHandle newDestination = await test.Funds.Create("New destination").In(july).WithGoal(1m).CreateAsync();
        TransactionHandle transfer = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 12)).For(10m).From(oldSource).To(oldDestination).CreateAsync();

        await test.Transactions.Fund().On(new DateOnly(2026, 7, 12)).For(10m).From(newSource).To(newDestination).UpdateAsync(transfer);

        await test.AssertBalanceAsync(oldSource, 0m, 0m);
        await test.AssertBalanceAsync(oldDestination, 0m, 0m);
        await test.AssertBalanceAsync(newSource, -10m, -10m);
        await test.AssertBalanceAsync(newDestination, 10m, 10m);
        Assert.Equal(10m, (await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{newDestination.Goal.Id}/progress/{july.Id}")).Contribution?.AssignedAmount);
    }
}
