using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for spending transaction history replay.
/// </summary>
public sealed class SpendingTransactionChronologyTests
{
    /// <summary>
    /// Reposting an earlier transaction rebuilds the balances and event history of a later posted transaction.
    /// </summary>
    [Fact]
    public async Task RepostEarlierTransactionReplaysLaterPostedBalanceHistories()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        SpendingTransactionBuilder earlierSpending = test.Transactions.Spending()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From(cash)
            .To("Market", groceries);
        SpendingTransactionBuilder laterSpending = test.Transactions.Spending()
            .In(july)
            .On(new DateOnly(2026, 7, 16))
            .For(120m)
            .From(cash)
            .To("Market", groceries);
        TransactionHandle earlier = await earlierSpending.CreateAsync();
        TransactionHandle later = await laterSpending.CreateAsync();

        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 20));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 21));
        await test.Transactions.UnpostAsync(earlier);
        await earlierSpending.For(100m).UpdateAsync(earlier);
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 20));

        AccountBalanceSnapshot account = await test.AccountQueries.GetBalanceAsync(cash);
        FundBalanceSnapshot fund = await test.FundQueries.GetBalanceAsync(groceries);
        AccountingPeriodBalanceSnapshot period = await test.AccountingPeriodQueries.GetBalanceAsync(july);
        FundGoalAvailabilitySnapshot goal = await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal);
        AccountBalanceEventSnapshot laterEvent = await test.AccountQueries.GetBalanceEventAsync(
            cash,
            later,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31));

        Assert.Equal(780m, account.Posted);
        Assert.Equal(780m, account.IncludingPending);
        Assert.Equal(-220m, fund.Posted);
        Assert.Equal(-220m, fund.IncludingPending);
        Assert.Equal(1000m, period.Opening);
        Assert.Equal(780m, period.Closing);
        Assert.Equal(-220m, goal.Posted);
        Assert.Equal(-220m, goal.IncludingPending);
        Assert.True(laterEvent.IsPosted);
        Assert.Equal(900m, laterEvent.Previous.Posted);
        Assert.Equal(780m, laterEvent.New.Posted);
    }
}
