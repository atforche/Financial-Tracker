using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for pending spending transaction balance effects.
/// </summary>
public sealed class PendingSpendingTransactionTests
{
    /// <summary>
    /// Pending spending affects effective account, fund, and fund goal balances without changing posted balances.
    /// </summary>
    [Fact]
    public async Task CreateAsyncWithUnpostedSpendingProjectsPendingEffectsAcrossBalanceSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        _ = await test.Transactions.Spending()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From(cash)
            .To("Market", groceries)
            .CreateAsync();

        AccountBalanceSnapshot account = await test.AccountQueries.GetBalanceAsync(cash);
        FundBalanceSnapshot fund = await test.FundQueries.GetBalanceAsync(groceries);
        AccountingPeriodBalanceSnapshot period = await test.AccountingPeriodQueries.GetBalanceAsync(july);
        FundGoalAvailabilitySnapshot goal = await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal);

        Assert.Equal(1000m, account.Posted);
        Assert.Equal(920m, account.IncludingPending);
        Assert.Equal(0m, fund.Posted);
        Assert.Equal(-80m, fund.IncludingPending);
        Assert.Equal(1000m, period.Opening);
        Assert.Equal(1000m, period.Closing);
        Assert.Equal(0m, goal.Posted);
        Assert.Equal(-80m, goal.IncludingPending);
    }
}
