using Models;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;
using Models.Transactions.Update;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers transaction calculation paths that require independently reconciling
/// account, period, fund, and fund-goal-total projections.
/// </summary>
public sealed class TransactionCalculationCoverageTests
{
    /// <summary>
    /// Verifies extra income funding increases funded-balance progress without
    /// satisfying the regular monthly contribution.
    /// </summary>
    [Fact]
    public async Task ExtraIncomeFundingCountsTowardFundedBalanceButNotRegularContribution()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle gifts = await test.Funds.Create("Gifts").In(july).CreateAsync();
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{gifts.Goal.Id}", new UpdateFundGoalModel
        {
            RegularContribution = 200m,
            MinimumFundedBalance = 50m,
            MaximumFundedBalance = 300m,
        });
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 10),
            Description = "Birthday gift",
            Amount = 50m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = new Models.Locations.LocationInputModel { NewLocationName = "Family" },
                IncomeLines = [new CreateIncomeLineModel { Description = "Gift", Amount = 50m }],
                IncomeDeductions = [],
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 50m,
                FundAssignments = [new CreateIncomeFundAmountModel
                {
                    FundId = gifts.Id,
                    Amount = 50m,
                    IsExtraContribution = true,
                }],
            }],
        });

        await test.Transactions.PostAsync(new TransactionHandle(created.Id), cash, new DateOnly(2026, 7, 10));
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{gifts.Goal.Id}/progress/{july.Id}");
        IncomeTransactionModel transaction = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{created.Id}");

        Assert.True(Assert.Single(Assert.Single(transaction.Destinations).FundAssignments).IsExtraContribution);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(0m, progress.Contribution.AssignedAmount);
        Assert.Equal(200m, progress.Contribution.RemainingAmount);
        Assert.NotNull(progress.FundedBalance);
        Assert.Equal(50m, progress.FundedBalance.Balance);
        Assert.Equal(FundedBalanceStatusModel.WithinRange, progress.FundedBalance.Status);
    }

    /// <summary>
    /// Verifies assigned and regular contribution totals remain distinct across
    /// income, spending, fund transfers, and lifecycle actions.
    /// </summary>
    [Fact]
    public async Task FundGoalTotalsReconcileAcrossIncomeSpendingTransferAndLifecycleActions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle reserve = await test.Funds.Create("Reserve").In(july).CreateAsync();
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel { RegularContribution = 100m });

        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(100m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 11)).For(30m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 12)).For(20m).From(groceries).To(reserve).CreateAsync();

        await AssertGoalAssignedAsync(test, july, groceries, 0m);
        FundGoalBalanceEventModel transferEvent = await GetGoalEventAsync(test, july, groceries, transfer: true);
        Assert.Equal(-20m, transferEvent.NewTotals.AmountAssigned);
        Assert.Equal(0m, transferEvent.NewTotals.AmountSpent);

        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        await AssertGoalAssignedAsync(test, july, groceries, 100m);

        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 11));
        await AssertGoalAssignedAsync(test, july, groceries, 100m);
        FundGoalBalanceEventModel spendingEvent = await GetGoalEventAsync(test, july, groceries, transfer: false);
        Assert.Equal(30m, spendingEvent.NewTotals.AmountSpent);

        await test.Transactions.UnpostAsync(spending);
        await test.Transactions.UnpostAsync(income);
        await AssertGoalAssignedAsync(test, july, groceries, 0m);

        await test.Transactions.DeleteAsync(spending);
        await test.Transactions.DeleteAsync(income);
        await AssertGoalAssignedAsync(test, july, groceries, 0m);
    }

    /// <summary>
    /// Verifies changing transaction parties removes every old pending account, fund, and goal effect.
    /// </summary>
    [Fact]
    public async Task UpdatingTransactionPartiesRemovesOldAccountFundAndGoalEffects()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountHandle third = await test.Accounts.Onboard("Third").CreateAsync();
        AccountHandle fourth = await test.Accounts.Onboard("Fourth").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle oldFund = await test.Funds.Create("Old").In(july).CreateAsync();
        FundHandle newFund = await test.Funds.Create("New").In(july).CreateAsync();
        FundHandle transferSource = await test.Funds.Create("Transfer source").In(july).CreateAsync();
        FundHandle transferDestination = await test.Funds.Create("Transfer destination").In(july).CreateAsync();
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{transferSource.Goal.Id}", new UpdateFundGoalModel { RegularContribution = 1m });
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{transferDestination.Goal.Id}", new UpdateFundGoalModel { RegularContribution = 1m });

        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(25m).From("Employer").To(first, oldFund).CreateAsync();
        UpdateTransactionModel incomeUpdate = new UpdateIncomeTransactionModel
        {
            Date = new DateOnly(2026, 7, 10),
            Description = "Income",
            Amount = 25m,
            Source = new UpdateIncomeTransactionSourceModel { Location = new Models.Locations.LocationInputModel { NewLocationName = "Employer" }, IncomeLines = [new UpdateIncomeLineModel { Description = "Pay", Amount = 25m }], IncomeDeductions = [] },
            Destinations = [new UpdateIncomeTransactionDestinationModel { AccountId = second.Id, Amount = 25m, FundAssignments = [new CreateIncomeFundAmountModel { FundId = newFund.Id, Amount = 25m }] }]
        };
        await test.Api.PostAsync($"/transactions/{income.Id}", incomeUpdate);
        Assert.Equal(0m, (await test.AccountQueries.GetBalanceAsync(first)).IncludingPending);
        Assert.Equal(25m, (await test.AccountQueries.GetBalanceAsync(second)).IncludingPending);
        await AssertFundAndGoalAsync(test, oldFund, 0m);
        await AssertFundAndGoalAsync(test, newFund, 25m);

        TransactionHandle accountTransfer = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 11)).For(20m).From(first).To(second).CreateAsync();
        UpdateTransactionModel accountTransferUpdate = new UpdateAccountTransactionModel
        {
            Date = new DateOnly(2026, 7, 11),
            Description = "Transfer",
            Amount = 20m,
            Source = new UpdateAccountTransactionSourceModel { AccountId = third.Id },
            Destinations = [new UpdateAccountTransactionDestinationModel { AccountId = fourth.Id, Amount = 20m }]
        };
        await test.Api.PostAsync($"/transactions/{accountTransfer.Id}", accountTransferUpdate);
        Assert.Equal(0m, (await test.AccountQueries.GetBalanceAsync(first)).IncludingPending);
        Assert.Equal(25m, (await test.AccountQueries.GetBalanceAsync(second)).IncludingPending);
        Assert.Equal(-20m, (await test.AccountQueries.GetBalanceAsync(third)).IncludingPending);
        Assert.Equal(20m, (await test.AccountQueries.GetBalanceAsync(fourth)).IncludingPending);

        TransactionHandle fundTransfer = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 12)).For(10m).From(oldFund).To(newFund).CreateAsync();
        UpdateTransactionModel fundTransferUpdate = new UpdateFundTransactionModel
        {
            Date = new DateOnly(2026, 7, 12),
            Description = "Fund transfer",
            Amount = 10m,
            Source = new UpdateFundTransactionSourceModel { FundId = transferSource.Id },
            Destinations = [new UpdateFundTransactionDestinationModel { FundId = transferDestination.Id, Amount = 10m }]
        };
        await test.Api.PostAsync($"/transactions/{fundTransfer.Id}", fundTransferUpdate);
        await AssertFundAndGoalAsync(test, oldFund, 0m);
        await AssertFundAndGoalAsync(test, newFund, 25m);
        await AssertFundAndGoalAsync(test, transferSource, -10m);
        await AssertFundAndGoalAsync(test, transferDestination, 10m);
        await AssertGoalAssignedAsync(test, july, transferSource, 0m);
        await AssertGoalAssignedAsync(test, july, transferDestination, 0m);
    }

    /// <summary>
    /// Verifies complex partial posting and a one-sided untracked transfer update both current and later period balances.
    /// </summary>
    [Fact]
    public async Task SplitPostingAndOneSidedUntrackedTransferReconcileCurrentAndLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await CreateUntrackedAccountAsync(test, "Card", july);
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateSpendingTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Split",
            Amount = 50m,
            Source = new CreateSpendingTransactionSourceModel { AccountId = cash.Id },
            Destinations = [
                new CreateSpendingTransactionDestinationModel { AccountId = card.Id, Amount = 30m, FundAssignments = [new CreateFundAmountModel { FundId = groceries.Id, Amount = 30m }] },
                new CreateSpendingTransactionDestinationModel { Location = new Models.Locations.LocationInputModel { NewLocationName = "Restaurant" }, Amount = 20m, FundAssignments = [new CreateFundAmountModel { FundId = dining.Id, Amount = 20m }] }
            ]
        });
        TransactionHandle spending = new(created.Id);

        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 16));
        await AssertPeriodBalancesAsync(test, july, august, 50m, 50m);
        await test.Transactions.PostAsync(spending, card, new DateOnly(2026, 7, 17));
        await AssertPeriodBalancesAsync(test, july, august, 80m, 80m);
        await test.Transactions.UnpostAsync(spending);
        await AssertPeriodBalancesAsync(test, july, august, 100m, 100m);
        await test.Transactions.DeleteAsync(spending);

        CreateTransactionResultModel oneSidedCreated = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateAccountTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 20),
            Description = "Card credit",
            Amount = 40m,
            Source = new CreateAccountTransactionSourceModel { Location = new Models.Locations.LocationInputModel { NewLocationName = "Adjustment" } },
            Destinations = [new CreateAccountTransactionDestinationModel { AccountId = card.Id, Amount = 40m }]
        });
        TransactionHandle oneSided = new(oneSidedCreated.Id);
        Assert.Equal(-40m, (await test.AccountQueries.GetBalanceAsync(card)).IncludingPending);
        await test.Transactions.PostAsync(oneSided, card, new DateOnly(2026, 7, 20));
        await AssertPeriodBalancesAsync(test, july, august, 140m, 140m);
        await test.Transactions.UnpostAsync(oneSided);
        Assert.Equal(0m, (await test.AccountQueries.GetBalanceAsync(card)).Posted);
        Assert.Equal(-40m, (await test.AccountQueries.GetBalanceAsync(card)).IncludingPending);
        await test.Transactions.DeleteAsync(oneSided);
        await AssertPeriodBalancesAsync(test, july, august, 100m, 100m);
    }

    private static async Task AssertGoalAssignedAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period, FundHandle fund, decimal assigned)
    {
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{fund.Goal.Id}/progress/{period.Id}");
        Assert.NotNull(progress.Contribution);
        Assert.Equal(assigned, progress.Contribution.AssignedAmount);
    }

    private static async Task<FundGoalBalanceEventModel> GetGoalEventAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period, FundHandle fund, bool transfer)
    {
        CollectionModel<FundGoalBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>($"/fund-goals/balance-events/accounting-period-range?range.start={period.Id}&range.end={period.Id}");
        return events.Items.Single(item => item.Fund.Id == fund.Id && (transfer ? item.Type == Models.BalanceEvents.BalanceEventTypeModel.Debit : item.Type == Models.BalanceEvents.BalanceEventTypeModel.Debit) && (transfer ? item.Source.DisplayName == fund.Name : item.Source.DisplayName == "Cash"));
    }

    private static async Task AssertFundAndGoalAsync(FinancialTrackerTestContext test, FundHandle fund, decimal expected)
    {
        Assert.Equal(expected, (await test.FundQueries.GetBalanceAsync(fund)).IncludingPending);
        Assert.Equal(expected, (await test.FundGoalQueries.GetAvailabilityAsync(fund.Goal)).IncludingPending);
    }

    private static async Task AssertPeriodBalancesAsync(FinancialTrackerTestContext test, AccountingPeriodHandle july, AccountingPeriodHandle august, decimal julyClosing, decimal augustOpening)
    {
        Assert.Equal(julyClosing, (await test.AccountingPeriodQueries.GetBalanceAsync(july)).Closing);
        Assert.Equal(augustOpening, (await test.AccountingPeriodQueries.GetBalanceAsync(august)).Opening);
    }

    private static async Task<AccountHandle> CreateUntrackedAccountAsync(FinancialTrackerTestContext test, string name, AccountingPeriodHandle period)
    {
        AccountModel account = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = name,
            Type = AccountTypeModel.Debt,
            OpeningAccountingPeriodId = period.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });
        return new AccountHandle(account.Id, account.Name);
    }
}