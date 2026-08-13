using Models.Accounts;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers partial posting for income transactions with multiple account destinations.
/// </summary>
public sealed class MultiDestinationIncomeTransactionTests
{
    /// <summary>
    /// Posts each income destination independently while retaining pending effects for the others.
    /// </summary>
    [Fact]
    public async Task PostAsyncKeepsUnpostedIncomeDestinationsPending()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Split income",
            Amount = 100m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Pay", Amount = 100m }],
                IncomeDeductions = []
            },
            Destinations = [
                new CreateIncomeTransactionDestinationModel { AccountId = first.Id, Amount = 40m, FundAssignments = [] },
                new CreateIncomeTransactionDestinationModel { AccountId = second.Id, Amount = 60m, FundAssignments = [] }
            ]
        });
        TransactionHandle transaction = new(created.Id);

        await test.Transactions.PostAsync(transaction, first, new DateOnly(2026, 7, 16));
        AccountBalanceSnapshot firstBalance = await test.AccountQueries.GetBalanceAsync(first);
        AccountBalanceSnapshot secondBalance = await test.AccountQueries.GetBalanceAsync(second);
        Assert.Equal(40m, firstBalance.Posted);
        Assert.Equal(40m, firstBalance.IncludingPending);
        Assert.Equal(0m, secondBalance.Posted);
        Assert.Equal(60m, secondBalance.IncludingPending);

        await test.Transactions.PostAsync(transaction, second, new DateOnly(2026, 7, 17));
        secondBalance = await test.AccountQueries.GetBalanceAsync(second);
        Assert.Equal(60m, secondBalance.Posted);
        Assert.Equal(60m, secondBalance.IncludingPending);
    }

    /// <summary>
    /// Propagates only posted destination effects into later Account and Fund period boundaries.
    /// </summary>
    [Fact]
    public async Task PartialPostingAsyncPropagatesOnlyPostedDestinationsToLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle firstFund = await test.Funds.Create("First fund").In(july).CreateAsync();
        FundHandle secondFund = await test.Funds.Create("Second fund").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Split income",
            Amount = 100m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Pay", Amount = 100m }],
                IncomeDeductions = []
            },
            Destinations = [
                new CreateIncomeTransactionDestinationModel
                {
                    AccountId = first.Id,
                    Amount = 40m,
                    FundAssignments = [new CreateIncomeFundAmountModel { FundId = firstFund.Id, Amount = 40m }]
                },
                new CreateIncomeTransactionDestinationModel
                {
                    AccountId = second.Id,
                    Amount = 60m,
                    FundAssignments = [new CreateIncomeFundAmountModel { FundId = secondFund.Id, Amount = 60m }]
                }
            ]
        });
        TransactionHandle transaction = new(created.Id);

        await test.Transactions.PostAsync(transaction, first, new DateOnly(2026, 7, 16));
        await AssertAugustBoundariesAsync(test, july, august, 40m, 40m);

        await test.Transactions.PostAsync(transaction, second, new DateOnly(2026, 7, 17));
        await AssertAugustBoundariesAsync(test, july, august, 100m, 100m);

        await test.Transactions.UnpostAsync(transaction);
        await AssertAugustBoundariesAsync(test, july, august, 0m, 0m);
    }

    private static async Task AssertAugustBoundariesAsync(
        FinancialTrackerTestContext test,
        AccountingPeriodHandle july,
        AccountingPeriodHandle august,
        decimal expectedAccountBalance,
        decimal expectedFundBalance)
    {
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        Assert.Equal(expectedAccountBalance, Assert.Single(accounts.AccountingPeriods,
            item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalBalance);
        Assert.Equal(expectedFundBalance, Assert.Single(funds.AccountingPeriods,
            item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalAssignedBalance);
    }
}