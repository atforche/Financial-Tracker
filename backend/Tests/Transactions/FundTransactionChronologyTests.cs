using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for chronological fund transfer balance replay.
/// </summary>
public sealed class FundTransactionChronologyTests
{
    /// <summary>
    /// Updating an earlier fund transfer replays later source and destination balances without changing accounts or periods.
    /// </summary>
    [Fact]
    public async Task UpdateEarlierTransferReplaysLaterFundAndFundGoalBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        FundTransactionBuilder earlier = test.Transactions.Fund()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From(groceries)
            .To(dining);
        FundTransactionBuilder later = test.Transactions.Fund()
            .In(july)
            .On(new DateOnly(2026, 7, 16))
            .For(120m)
            .From(groceries)
            .To(dining);
        TransactionHandle earlierTransaction = await earlier.CreateAsync();
        _ = await later.CreateAsync();

        await earlier.For(100m).UpdateAsync(earlierTransaction);

        AccountBalanceSnapshot account = await test.AccountQueries.GetBalanceAsync(cash);
        AccountingPeriodBalanceSnapshot period = await test.AccountingPeriodQueries.GetBalanceAsync(july);
        FundBalanceSnapshot source = await test.FundQueries.GetBalanceAsync(groceries);
        FundBalanceSnapshot destination = await test.FundQueries.GetBalanceAsync(dining);
        FundGoalAvailabilitySnapshot sourceGoal = await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal);
        FundGoalAvailabilitySnapshot destinationGoal = await test.FundGoalQueries.GetAvailabilityAsync(dining.Goal);

        Assert.Equal(1000m, account.Posted);
        Assert.Equal(1000m, account.IncludingPending);
        Assert.Equal(1000m, period.Opening);
        Assert.Equal(1000m, period.Closing);
        Assert.Equal(-220m, source.Posted);
        Assert.Equal(-220m, source.IncludingPending);
        Assert.Equal(220m, destination.Posted);
        Assert.Equal(220m, destination.IncludingPending);
        Assert.Equal(-220m, sourceGoal.Posted);
        Assert.Equal(-220m, sourceGoal.IncludingPending);
        Assert.Equal(220m, destinationGoal.Posted);
        Assert.Equal(220m, destinationGoal.IncludingPending);
    }
}