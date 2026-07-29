using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.FundGoals;

/// <summary>
/// Covers pending Fund Goal effects through the public API.
/// </summary>
public sealed class PendingFundGoalTotalsServiceTests
{
    /// <summary>
    /// Creates and removes a pending effect through transaction mutation endpoints.
    /// </summary>
    [Fact]
    public async Task TransactionEndpointsProjectAndRemovePendingFundGoalEffects()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();

        FundGoalAvailabilitySnapshot pending = await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal);
        await test.Api.DeleteAsync($"/transactions/{income.Id}");
        FundGoalAvailabilitySnapshot deleted = await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal);

        Assert.Equal(40m, pending.IncludingPending);
        Assert.Equal(0m, deleted.IncludingPending);
    }
}