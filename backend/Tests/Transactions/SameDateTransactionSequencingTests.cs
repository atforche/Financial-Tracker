using Models;
using Models.Accounts;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers balance-history replay when an earlier same-day transaction is rebuilt.
/// </summary>
public sealed class SameDateTransactionSequencingTests
{
    /// <summary>
    /// Reposting an updated same-day transaction keeps subsequent balances and event ordering deterministic.
    /// </summary>
    [Fact]
    public async Task RepostingUpdatedSameDateTransactionReplaysLaterHistoryInSequenceOrder()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        SpendingTransactionBuilder earlier = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("First", groceries);
        SpendingTransactionBuilder later = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Second", groceries);
        TransactionHandle earlierHandle = await earlier.CreateAsync();
        TransactionHandle laterHandle = await later.CreateAsync();
        await test.Transactions.PostAsync(earlierHandle, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(laterHandle, cash, new DateOnly(2026, 7, 15));

        await test.Transactions.UnpostAsync(earlierHandle);
        await earlier.For(15m).UpdateAsync(earlierHandle);
        await test.Transactions.PostAsync(earlierHandle, cash, new DateOnly(2026, 7, 15));

        AccountBalanceSnapshot balance = await test.AccountQueries.GetBalanceAsync(cash);
        CollectionModel<AccountBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-15&range.end=2026-07-15&sort=Date");
        AccountBalanceEventModel[] matchingEvents = events.Items
            .Where(item => item.TransactionId == earlierHandle.Id || item.TransactionId == laterHandle.Id)
            .ToArray();

        Assert.Equal(65m, balance.Posted);
        Assert.Equal(65m, balance.IncludingPending);
        Assert.Equal([laterHandle.Id, earlierHandle.Id], matchingEvents.Select(item => item.TransactionId));
        Assert.Equal(80m, matchingEvents[0].NewBalance.PostedBalance);
        Assert.Equal(65m, matchingEvents[1].NewBalance.PostedBalance);
    }
}
