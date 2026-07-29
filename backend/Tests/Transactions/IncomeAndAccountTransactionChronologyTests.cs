using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Regression coverage for transaction-history replay beyond spending and Fund transfers.
/// </summary>
public sealed class IncomeAndAccountTransactionChronologyTests
{
    /// <summary>
    /// Reposting an earlier income rebuilds the later posted account, Fund, goal, and period histories.
    /// </summary>
    [Fact]
    public async Task RepostEarlierIncomeReplaysLaterPostedBalanceHistories()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        IncomeTransactionBuilder earlierBuilder = test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From("Employer").To(cash, income);
        IncomeTransactionBuilder laterBuilder = test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 16)).For(30m).From("Employer").To(cash, income);
        TransactionHandle earlier = await earlierBuilder.CreateAsync();
        TransactionHandle later = await laterBuilder.CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 16));

        await test.Transactions.UnpostAsync(earlier);
        await earlierBuilder.For(25m).UpdateAsync(earlier);
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 15));

        AccountBalanceSnapshot account = await test.AccountQueries.GetBalanceAsync(cash);
        FundBalanceSnapshot fund = await test.FundQueries.GetBalanceAsync(income);
        FundGoalAvailabilitySnapshot goal = await test.FundGoalQueries.GetAvailabilityAsync(income.Goal);
        AccountingPeriodBalanceSnapshot period = await test.AccountingPeriodQueries.GetBalanceAsync(july);

        Assert.Equal(155m, account.Posted);
        Assert.Equal(55m, fund.Posted);
        Assert.Equal(55m, goal.Posted);
        Assert.Equal(155m, period.Closing);
    }

    /// <summary>
    /// Reposting an earlier account transfer rebuilds later source and destination histories.
    /// </summary>
    [Fact]
    public async Task RepostEarlierAccountTransferReplaysLaterPostedBalanceHistories()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle checking = await test.Accounts.Onboard("Checking").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").WithOpeningBalance(50m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountTransactionBuilder earlierBuilder = test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(checking).To(savings);
        AccountTransactionBuilder laterBuilder = test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 16)).For(30m).From(checking).To(savings);
        TransactionHandle earlier = await earlierBuilder.CreateAsync();
        TransactionHandle later = await laterBuilder.CreateAsync();
        await test.Transactions.PostAsync(earlier, checking, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(earlier, savings, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(later, checking, new DateOnly(2026, 7, 16));
        await test.Transactions.PostAsync(later, savings, new DateOnly(2026, 7, 16));

        await test.Transactions.UnpostAsync(earlier);
        await earlierBuilder.For(25m).UpdateAsync(earlier);
        await test.Transactions.PostAsync(earlier, checking, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(earlier, savings, new DateOnly(2026, 7, 15));

        AccountBalanceSnapshot checkingBalance = await test.AccountQueries.GetBalanceAsync(checking);
        AccountBalanceSnapshot savingsBalance = await test.AccountQueries.GetBalanceAsync(savings);

        Assert.Equal(45m, checkingBalance.Posted);
        Assert.Equal(105m, savingsBalance.Posted);
    }
}
