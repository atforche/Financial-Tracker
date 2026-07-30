using Models.Accounts;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Update;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Exercises transaction shapes whose balance effects differ from the single-party lifecycle cases.
/// </summary>
public sealed class TransactionCalculationGapTests
{
    /// <summary>Verifies split spending routes posted and pending effects by account and destination.</summary>
    [Fact]
    public async Task SplitSpendingToAccountAndLocationKeepsAllBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle card = await CreateUntrackedAccountAsync(test, "Card");
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        TransactionHandle transaction = await CreateSpendingAsync(test, july, cash, [
            new CreateSpendingTransactionDestinationModel { AccountId = card.Id, Amount = 30m, FundAssignments = [new CreateFundAmountModel { FundId = groceries.Id, Amount = 30m }] },
            new CreateSpendingTransactionDestinationModel { Location = "Restaurant", Amount = 20m, FundAssignments = [new CreateFundAmountModel { FundId = dining.Id, Amount = 20m }] }
        ]);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        await AssertBalancesAsync(test, cash, card, groceries, dining, 50m, 50m, 0m, -30m, 0m, -30m, -20m, -20m);

        await test.Transactions.PostAsync(transaction, card, new DateOnly(2026, 7, 17));
        await AssertBalancesAsync(test, cash, card, groceries, dining, 50m, 50m, -30m, -30m, -30m, -30m, -20m, -20m);

        await test.Transactions.UnpostAsync(transaction);
        await AssertBalancesAsync(test, cash, card, groceries, dining, 100m, 50m, 0m, -30m, 0m, -30m, 0m, -20m);
    }

    /// <summary>Verifies an untracked income source posts independently from its tracked destination.</summary>
    [Fact]
    public async Task IncomeFromUntrackedAccountPostsSourceDestinationFundsAndGoalsIndependently()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(10m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await CreateUntrackedAccountAsync(test, "Card", july);
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        TransactionHandle transaction = await CreateIncomeAsync(test, july, card.Id, cash, income, 40m);

        await test.Transactions.PostAsync(transaction, card, new DateOnly(2026, 7, 16));
        Assert.Equal(40m, (await test.AccountQueries.GetBalanceAsync(card)).Posted);
        Assert.Equal(10m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
        Assert.Equal(0m, (await test.FundQueries.GetBalanceAsync(income)).Posted);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 17));
        Assert.Equal(40m, (await test.AccountQueries.GetBalanceAsync(card)).Posted);
        Assert.Equal(50m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
        Assert.Equal(40m, (await test.FundQueries.GetBalanceAsync(income)).Posted);
        Assert.Equal(40m, (await test.FundGoalQueries.GetAvailabilityAsync(income.Goal)).Posted);
    }

    /// <summary>Verifies updating an allocation removes all old fund and goal effects.</summary>
    [Fact]
    public async Task UpdatingFundAssignmentsMovesPendingAndPostedEffectsWithoutLeavingOldFundBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle oldFund = await test.Funds.Create("Old").In(july).CreateAsync();
        FundHandle newFund = await test.Funds.Create("New").In(july).CreateAsync();
        TransactionHandle transaction = await CreateSpendingAsync(test, july, cash, [
            new CreateSpendingTransactionDestinationModel { Location = "Market", Amount = 25m, FundAssignments = [new CreateFundAmountModel { FundId = oldFund.Id, Amount = 25m }] }
        ]);

        UpdateTransactionModel update = new UpdateSpendingTransactionModel
        {
            Date = new DateOnly(2026, 7, 16),
            Description = "Market",
            Amount = 25m,
            Source = new UpdateSpendingTransactionSourceModel { AccountId = cash.Id },
            Destinations = [new UpdateSpendingTransactionDestinationModel { Location = "Market", Amount = 25m, FundAssignments = [new CreateFundAmountModel { FundId = newFund.Id, Amount = 25m }] }]
        };
        await test.Api.PostAsync($"/transactions/{transaction.Id}", update);
        Assert.Equal(0m, (await test.FundQueries.GetBalanceAsync(oldFund)).IncludingPending);
        Assert.Equal(-25m, (await test.FundQueries.GetBalanceAsync(newFund)).IncludingPending);
        Assert.Equal(0m, (await test.FundGoalQueries.GetAvailabilityAsync(oldFund.Goal)).IncludingPending);
        Assert.Equal(-25m, (await test.FundGoalQueries.GetAvailabilityAsync(newFund.Goal)).IncludingPending);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        Assert.Equal(0m, (await test.FundQueries.GetBalanceAsync(oldFund)).Posted);
        Assert.Equal(-25m, (await test.FundQueries.GetBalanceAsync(newFund)).Posted);
    }

    /// <summary>Verifies partial income posting keeps fund-goal posted and pending totals distinct.</summary>
    [Fact]
    public async Task PartiallyPostedSplitIncomeSeparatesFundGoalPostedAndPendingTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle firstFund = await test.Funds.Create("First fund").In(july).CreateAsync();
        FundHandle secondFund = await test.Funds.Create("Second fund").In(july).CreateAsync();
        TransactionHandle transaction = await CreateSplitIncomeAsync(test, july, first, firstFund, second, secondFund);

        await test.Transactions.PostAsync(transaction, first, new DateOnly(2026, 7, 16));
        await AssertGoalAsync(test, firstFund, 40m, 40m);
        await AssertGoalAsync(test, secondFund, 0m, 60m);

        await test.Transactions.UnpostAsync(transaction);
        await AssertGoalAsync(test, firstFund, 0m, 40m);
        await AssertGoalAsync(test, secondFund, 0m, 60m);

        await test.Transactions.DeleteAsync(transaction);
        await AssertGoalAsync(test, firstFund, 0m, 0m);
        await AssertGoalAsync(test, secondFund, 0m, 0m);
    }

    /// <summary>Verifies every destination of a split fund transfer receives and reverses its effect.</summary>
    [Fact]
    public async Task SplitFundTransferUpdatesEveryDestinationAndGoalThenReversesOnDelete()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle source = await test.Funds.Create("Source").In(july).CreateAsync();
        FundHandle first = await test.Funds.Create("First").In(july).CreateAsync();
        FundHandle second = await test.Funds.Create("Second").In(july).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateFundTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Split",
            Amount = 50m,
            Source = new CreateFundTransactionSourceModel { FundId = source.Id },
            Destinations = [new CreateFundTransactionDestinationModel { FundId = first.Id, Amount = 20m }, new CreateFundTransactionDestinationModel { FundId = second.Id, Amount = 30m }]
        });
        TransactionHandle transaction = new(created.Id);
        await AssertFundAndGoalAsync(test, source, -50m);
        await AssertFundAndGoalAsync(test, first, 20m);
        await AssertFundAndGoalAsync(test, second, 30m);

        await test.Transactions.DeleteAsync(transaction);
        await AssertFundAndGoalAsync(test, source, 0m);
        await AssertFundAndGoalAsync(test, first, 0m);
        await AssertFundAndGoalAsync(test, second, 0m);
    }

    /// <summary>Verifies moving an earlier transaction replays later fund and goal histories without duplication.</summary>
    [Fact]
    public async Task MovingEarlierIncomeDateReplaysLaterPeriodFundGoalTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        TransactionHandle earlier = await CreateIncomeAsync(test, july, null, cash, income, 20m, new DateOnly(2026, 7, 20));
        TransactionHandle later = await CreateIncomeAsync(test, july, null, cash, income, 30m, new DateOnly(2026, 7, 21));
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 20));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 21));
        await test.Transactions.UnpostAsync(earlier);
        UpdateTransactionModel update = CreateIncomeUpdate(cash, income, 25m, new DateOnly(2026, 7, 15));
        await test.Api.PostAsync($"/transactions/{earlier.Id}", update);
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 15));

        Assert.Equal(55m, (await test.FundQueries.GetBalanceAsync(income)).Posted);
        Assert.Equal(55m, (await test.FundGoalQueries.GetAvailabilityAsync(income.Goal)).Posted);
    }

    /// <summary>Verifies a representative cross-surface action sequence reaches one reconciled state.</summary>
    [Fact]
    public async Task RepresentativeTransactionActionSequenceReconcilesAllCurrentBalanceSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle pay = await CreateIncomeAsync(test, july, null, cash, income, 60m);
        TransactionHandle spend = await CreateSpendingAsync(test, july, cash, [new CreateSpendingTransactionDestinationModel { Location = "Market", Amount = 15m, FundAssignments = [new CreateFundAmountModel { FundId = groceries.Id, Amount = 15m }] }]);
        await test.Transactions.PostAsync(pay, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(spend, cash, new DateOnly(2026, 7, 16));
        await test.Transactions.UnpostAsync(spend);
        await test.Transactions.DeleteAsync(spend);

        Assert.Equal(160m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
        Assert.Equal(160m, (await test.AccountQueries.GetBalanceAsync(cash)).IncludingPending);
        await AssertFundAndGoalAsync(test, income, 60m);
        await AssertFundAndGoalAsync(test, groceries, 0m);
        AccountingPeriodBalanceSnapshot period = await test.AccountingPeriodQueries.GetBalanceAsync(july);
        Assert.Equal(100m, period.Opening);
        Assert.Equal(160m, period.Closing);
    }

    private static async Task<AccountHandle> CreateUntrackedAccountAsync(FinancialTrackerTestContext test, string name, AccountingPeriodHandle? period = null)
    {
        if (period == null)
        {
            period = await test.Periods.Create(2026, 6).CreateAsync();
        }
        AccountModel account = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel { Name = name, Type = AccountTypeModel.Debt, OpeningAccountingPeriodId = period.Id, DateOpened = new DateOnly(2026, 7, 1) });
        return new AccountHandle(account.Id, account.Name);
    }

    private static async Task<TransactionHandle> CreateSpendingAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period, AccountHandle source, IReadOnlyCollection<CreateSpendingTransactionDestinationModel> destinations)
    {
        decimal amount = destinations.Sum(destination => destination.Amount);
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateSpendingTransactionModel { AccountingPeriodId = period.Id, Date = new DateOnly(2026, 7, 15), Description = "Spending", Amount = amount, Source = new CreateSpendingTransactionSourceModel { AccountId = source.Id }, Destinations = destinations });
        return new TransactionHandle(created.Id);
    }

    private static async Task<TransactionHandle> CreateIncomeAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period, Guid? sourceAccountId, AccountHandle destination, FundHandle fund, decimal amount, DateOnly? date = null)
    {
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel { AccountingPeriodId = period.Id, Date = date ?? new DateOnly(2026, 7, 15), Description = "Income", Amount = amount, Source = new CreateIncomeTransactionSourceModel { AccountId = sourceAccountId, Location = sourceAccountId == null ? "Employer" : null, IncomeLines = [new CreateIncomeLineModel { Description = "Pay", Amount = amount }], IncomeDeductions = [] }, Destinations = [new CreateIncomeTransactionDestinationModel { AccountId = destination.Id, Amount = amount, FundAssignments = [new CreateFundAmountModel { FundId = fund.Id, Amount = amount }] }] });
        return new TransactionHandle(created.Id);
    }

    private static async Task<TransactionHandle> CreateSplitIncomeAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period, AccountHandle first, FundHandle firstFund, AccountHandle second, FundHandle secondFund)
    {
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel { AccountingPeriodId = period.Id, Date = new DateOnly(2026, 7, 15), Description = "Income", Amount = 100m, Source = new CreateIncomeTransactionSourceModel { Location = "Employer", IncomeLines = [new CreateIncomeLineModel { Description = "Pay", Amount = 100m }], IncomeDeductions = [] }, Destinations = [new CreateIncomeTransactionDestinationModel { AccountId = first.Id, Amount = 40m, FundAssignments = [new CreateFundAmountModel { FundId = firstFund.Id, Amount = 40m }] }, new CreateIncomeTransactionDestinationModel { AccountId = second.Id, Amount = 60m, FundAssignments = [new CreateFundAmountModel { FundId = secondFund.Id, Amount = 60m }] }] });
        return new TransactionHandle(created.Id);
    }

    private static UpdateIncomeTransactionModel CreateIncomeUpdate(AccountHandle account, FundHandle fund, decimal amount, DateOnly date) => new() { Date = date, Description = "Income", Amount = amount, Source = new UpdateIncomeTransactionSourceModel { Location = "Employer", IncomeLines = [new UpdateIncomeLineModel { Description = "Pay", Amount = amount }], IncomeDeductions = [] }, Destinations = [new UpdateIncomeTransactionDestinationModel { AccountId = account.Id, Amount = amount, FundAssignments = [new CreateFundAmountModel { FundId = fund.Id, Amount = amount }] }] };

    private static async Task AssertGoalAsync(FinancialTrackerTestContext test, FundHandle fund, decimal posted, decimal includingPending)
    {
        FundGoalAvailabilitySnapshot goal = await test.FundGoalQueries.GetAvailabilityAsync(fund.Goal);
        Assert.Equal(posted, goal.Posted);
        Assert.Equal(includingPending, goal.IncludingPending);
    }

    private static async Task AssertFundAndGoalAsync(FinancialTrackerTestContext test, FundHandle fund, decimal expected)
    {
        FundBalanceSnapshot balance = await test.FundQueries.GetBalanceAsync(fund);
        Assert.Equal(expected, balance.Posted);
        Assert.Equal(expected, balance.IncludingPending);
        await AssertGoalAsync(test, fund, expected, expected);
    }

    private static async Task AssertBalancesAsync(FinancialTrackerTestContext test, AccountHandle cash, AccountHandle card, FundHandle groceries, FundHandle dining, decimal cashPosted, decimal cashPending, decimal cardPosted, decimal cardPending, decimal groceriesPosted, decimal groceriesPending, decimal diningPosted, decimal diningPending)
    {
        AccountBalanceSnapshot cashBalance = await test.AccountQueries.GetBalanceAsync(cash);
        AccountBalanceSnapshot cardBalance = await test.AccountQueries.GetBalanceAsync(card);
        Assert.Equal(cashPosted, cashBalance.Posted);
        Assert.Equal(cashPending, cashBalance.IncludingPending);
        Assert.Equal(cardPosted, cardBalance.Posted);
        Assert.Equal(cardPending, cardBalance.IncludingPending);
        Assert.Equal(groceriesPosted, (await test.FundQueries.GetBalanceAsync(groceries)).Posted);
        Assert.Equal(groceriesPending, (await test.FundQueries.GetBalanceAsync(groceries)).IncludingPending);
        await AssertGoalAsync(test, groceries, groceriesPosted, groceriesPending);
        await AssertGoalAsync(test, dining, diningPosted, diningPending);
    }
}