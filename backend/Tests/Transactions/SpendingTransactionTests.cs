using Models;
using Models.Accounts;

namespace Tests.Transactions;

/// <summary>
/// Covers how spending affects account, fund, and accounting-period balances.
/// </summary>
public sealed class SpendingTransactionTests
{
    /// <summary>
    /// Creating, updating, posting, unposting, and deleting spending keeps each balance surface synchronized.
    /// </summary>
    [Fact]
    public async Task UpdatePostUnpostAndDeleteAsyncKeepBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        SpendingTransactionBuilder spending = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(80m).From(cash).To("Market", groceries);
        TransactionHandle transaction = await spending.CreateAsync();
        await test.AssertBalanceAsync(cash, 1000m, 920m);
        await test.AssertBalanceAsync(groceries, 0m, -80m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);

        await spending.For(125m).UpdateAsync(transaction);
        await test.AssertBalanceAsync(cash, 1000m, 875m);
        await test.AssertBalanceAsync(groceries, 0m, -125m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 20));
        await test.AssertBalanceAsync(cash, 875m, 875m);
        await test.AssertBalanceAsync(groceries, -125m, -125m);
        await test.AssertBalanceAsync(july, 1000m, 875m);

        await test.Transactions.UnpostAsync(transaction);
        await test.AssertBalanceAsync(cash, 1000m, 875m);
        await test.AssertBalanceAsync(groceries, 0m, -125m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(cash, 1000m, 1000m);
        await test.AssertBalanceAsync(groceries, 0m, 0m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);
    }

    /// <summary>
    /// Pending and posted spending from a credit card increases its balance.
    /// </summary>
    [Fact]
    public async Task CreditCardSpendingIncreasesPostedAndPendingBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await test.Accounts.Create("Card", july).WithType(AccountTypeModel.CreditCard).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(card).To("Market", groceries).CreateAsync();
        await test.AssertBalanceAsync(card, 0m, 25m);

        await test.Transactions.PostAsync(transaction, card, new DateOnly(2026, 7, 15));
        await test.AssertBalanceAsync(card, 25m, 25m);
    }

    /// <summary>
    /// Split spending to an account and a location posts each side's account and fund effects independently,
    /// including into later periods.
    /// </summary>
    [Fact]
    public async Task SplitSpendingToAccountAndLocationPostsEachSideIndependently()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await test.Accounts.Create("Card", july).WithType(AccountTypeModel.Debt).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(50m)
            .From(cash).To(card, groceries, 30m).To("Restaurant", dining, 20m).CreateAsync();

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(cash, 50m, 50m);
        await test.AssertBalanceAsync(card, 0m, -30m);
        await test.AssertBalanceAsync(groceries, 0m, -30m);
        await test.AssertBalanceAsync(dining, -20m, -20m);
        await test.AssertBalanceAsync(july, 100m, 50m);
        await test.AssertBalanceAsync(august, 50m, 50m);

        await test.Transactions.PostAsync(transaction, card, new DateOnly(2026, 7, 17));
        await test.AssertBalanceAsync(cash, 50m, 50m);
        await test.AssertBalanceAsync(card, -30m, -30m);
        await test.AssertBalanceAsync(groceries, -30m, -30m);
        await test.AssertBalanceAsync(dining, -20m, -20m);
        await test.AssertBalanceAsync(july, 100m, 80m);
        await test.AssertBalanceAsync(august, 80m, 80m);

        await test.Transactions.UnpostAsync(transaction);
        await test.AssertBalanceAsync(cash, 100m, 50m);
        await test.AssertBalanceAsync(card, 0m, -30m);
        await test.AssertBalanceAsync(groceries, 0m, -30m);
        await test.AssertBalanceAsync(dining, 0m, -20m);
        await test.AssertBalanceAsync(july, 100m, 100m);
        await test.AssertBalanceAsync(august, 100m, 100m);
    }

    /// <summary>
    /// Changing spending's fund assignment moves its pending and posted effects without leaving old fund balances.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncMovesEffectsToNewFund()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle oldFund = await test.Funds.Create("Old").In(july).CreateAsync();
        FundHandle newFund = await test.Funds.Create("New").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(cash).To("Market", oldFund).CreateAsync();

        await test.Transactions.Spending().On(new DateOnly(2026, 7, 16)).For(25m).From(cash).To("Market", newFund).UpdateAsync(transaction);
        await test.AssertBalanceAsync(oldFund, 0m, 0m);
        await test.AssertBalanceAsync(newFund, 0m, -25m);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(oldFund, 0m, 0m);
        await test.AssertBalanceAsync(newFund, -25m, -25m);
    }

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
        SpendingTransactionBuilder earlierSpending = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(80m).From(cash).To("Market", groceries);
        TransactionHandle earlier = await earlierSpending.CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 16)).For(120m).From(cash).To("Market", groceries).CreateAsync();

        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 20));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 21));
        await test.Transactions.UnpostAsync(earlier);
        await earlierSpending.For(100m).UpdateAsync(earlier);
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 20));

        await test.AssertBalanceAsync(cash, 780m, 780m);
        await test.AssertBalanceAsync(groceries, -220m, -220m);
        await test.AssertBalanceAsync(july, 1000m, 780m);
        CollectionModel<AccountBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31");
        AccountBalanceEventModel laterEvent = Assert.Single(events.Items, item => item.TransactionId == later.Id);
        Assert.True(laterEvent.IsPosted);
        Assert.Equal(900m, laterEvent.PreviousBalance?.PostedBalance);
        Assert.Equal(780m, laterEvent.NewBalance?.PostedBalance);
    }

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
        SpendingTransactionBuilder earlierBuilder = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("First", groceries);
        TransactionHandle earlier = await earlierBuilder.CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Second", groceries).CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 15));

        await test.Transactions.UnpostAsync(earlier);
        await earlierBuilder.For(15m).UpdateAsync(earlier);
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 15));

        await test.AssertBalanceAsync(cash, 65m, 65m);
        CollectionModel<AccountBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-15&range.end=2026-07-15&sort=Date");
        Assert.Equal([later.Id, earlier.Id], events.Items.Select(item => item.TransactionId));
        Assert.Equal([80m, 65m], events.Items.Select(item => item.NewBalance?.PostedBalance));
    }
}
