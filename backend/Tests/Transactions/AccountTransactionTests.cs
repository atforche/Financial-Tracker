using Models.Accounts;

namespace Tests.Transactions;

/// <summary>
/// Covers how account transfers affect account, fund, and accounting-period balances.
/// </summary>
public sealed class AccountTransactionTests
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
        AccountTransactionBuilder transfer = test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 15)).For(80m).From(checking).To(savings);
        TransactionHandle transaction = await transfer.CreateAsync();

        await transfer.For(125m).UpdateAsync(transaction);
        await test.AssertBalanceAsync(checking, 1000m, 875m);
        await test.AssertBalanceAsync(savings, 500m, 625m);
        await test.AssertBalanceAsync(july, 1500m, 1500m);
        await test.AssertBalanceAsync(reserve, 0m, 0m);

        await test.Transactions.PostAsync(transaction, checking, new DateOnly(2026, 7, 20));
        await test.AssertBalanceAsync(checking, 875m, 875m);
        await test.AssertBalanceAsync(savings, 500m, 625m);
        await test.AssertBalanceAsync(july, 1500m, 1375m);
        await test.AssertBalanceAsync(reserve, 0m, 0m);

        await test.Transactions.PostAsync(transaction, savings, new DateOnly(2026, 7, 21));
        await test.AssertBalanceAsync(checking, 875m, 875m);
        await test.AssertBalanceAsync(savings, 625m, 625m);
        await test.AssertBalanceAsync(july, 1500m, 1500m);
        await test.AssertBalanceAsync(reserve, 0m, 0m);

        await test.Transactions.UnpostAsync(transaction);
        await test.AssertBalanceAsync(checking, 1000m, 875m);
        await test.AssertBalanceAsync(savings, 500m, 625m);
        await test.AssertBalanceAsync(july, 1500m, 1500m);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(checking, 1000m, 1000m);
        await test.AssertBalanceAsync(savings, 500m, 500m);
        await test.AssertBalanceAsync(july, 1500m, 1500m);
        await test.AssertBalanceAsync(reserve, 0m, 0m);
    }

    /// <summary>
    /// Posts each affected account independently while retaining pending effects for the rest.
    /// </summary>
    [Fact]
    public async Task PostAsyncKeepsUnpostedDestinationsPending()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle source = await test.Accounts.Onboard("Source").WithOpeningBalance(1000m).CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 15)).For(100m)
            .From(source).To(first, 40m).To(second, 60m).CreateAsync();

        await test.Transactions.PostAsync(transaction, source, new DateOnly(2026, 7, 20));
        await test.AssertBalanceAsync(source, 900m, 900m);
        await test.AssertBalanceAsync(first, 0m, 40m);
        await test.AssertBalanceAsync(second, 0m, 60m);

        await test.Transactions.PostAsync(transaction, first, new DateOnly(2026, 7, 21));
        await test.Transactions.PostAsync(transaction, second, new DateOnly(2026, 7, 22));
        await test.AssertBalanceAsync(first, 40m, 40m);
        await test.AssertBalanceAsync(second, 60m, 60m);

        await test.Transactions.UnpostAsync(transaction);
        await test.AssertBalanceAsync(source, 1000m, 900m);
        await test.AssertBalanceAsync(first, 0m, 40m);
        await test.AssertBalanceAsync(second, 0m, 60m);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(source, 1000m, 1000m);
        await test.AssertBalanceAsync(first, 0m, 0m);
        await test.AssertBalanceAsync(second, 0m, 0m);
    }

    /// <summary>
    /// Reposting an earlier transfer rebuilds later source and destination histories.
    /// </summary>
    [Fact]
    public async Task RepostEarlierTransferReplaysLaterPostedBalanceHistories()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle checking = await test.Accounts.Onboard("Checking").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").WithOpeningBalance(50m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountTransactionBuilder earlierBuilder = test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(checking).To(savings);
        TransactionHandle earlier = await earlierBuilder.CreateAsync();
        TransactionHandle later = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 16)).For(30m).From(checking).To(savings).CreateAsync();
        await test.Transactions.PostAsync(earlier, checking, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(earlier, savings, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(later, checking, new DateOnly(2026, 7, 16));
        await test.Transactions.PostAsync(later, savings, new DateOnly(2026, 7, 16));

        await test.Transactions.UnpostAsync(earlier);
        await earlierBuilder.For(25m).UpdateAsync(earlier);
        await test.Transactions.PostAsync(earlier, checking, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(earlier, savings, new DateOnly(2026, 7, 15));

        await test.AssertBalanceAsync(checking, 45m, 45m);
        await test.AssertBalanceAsync(savings, 105m, 105m);
    }

    /// <summary>
    /// Changing a transfer's accounts removes every pending effect from the old accounts.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncMovesEffectsToNewAccounts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountHandle third = await test.Accounts.Onboard("Third").CreateAsync();
        AccountHandle fourth = await test.Accounts.Onboard("Fourth").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        TransactionHandle transfer = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 11)).For(20m).From(first).To(second).CreateAsync();

        await test.Transactions.Account().On(new DateOnly(2026, 7, 11)).For(20m).From(third).To(fourth).UpdateAsync(transfer);

        await test.AssertBalanceAsync(first, 0m, 0m);
        await test.AssertBalanceAsync(second, 0m, 0m);
        await test.AssertBalanceAsync(third, 0m, -20m);
        await test.AssertBalanceAsync(fourth, 0m, 20m);
    }

    /// <summary>
    /// A transfer from an untracked location affects only its destination and the period balances.
    /// </summary>
    [Fact]
    public async Task OneSidedTransferFromLocationReconcilesCurrentAndLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await test.Accounts.Create("Card", july).WithType(AccountTypeModel.Debt).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        TransactionHandle transfer = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 20)).For(40m).From("Adjustment").To(card).CreateAsync();
        await test.AssertBalanceAsync(card, 0m, -40m);

        await test.Transactions.PostAsync(transfer, card, new DateOnly(2026, 7, 20));
        await test.AssertBalanceAsync(july, 100m, 140m);
        await test.AssertBalanceAsync(august, 140m, 140m);

        await test.Transactions.UnpostAsync(transfer);
        await test.AssertBalanceAsync(card, 0m, -40m);

        await test.Transactions.DeleteAsync(transfer);
        await test.AssertBalanceAsync(july, 100m, 100m);
        await test.AssertBalanceAsync(august, 100m, 100m);
    }
}
