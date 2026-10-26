using System.Net;
using Models;
using Models.Accounts;
using Models.Funds;
using Models.Locations;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;

namespace Tests.Transactions;

/// <summary>
/// Covers the requests and lifecycle operations that transaction mutations reject.
/// </summary>
public sealed class TransactionValidationTests
{
    /// <summary>
    /// Rejects an invalid destination total and posting through an unaffected account.
    /// </summary>
    [Fact]
    public async Task MutationsRejectMismatchedAmountsAndUnaffectedPostingAccounts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle source = await test.Accounts.Onboard("Source").WithOpeningBalance(100m).CreateAsync();
        AccountHandle destination = await test.Accounts.Onboard("Destination").CreateAsync();
        AccountHandle unrelated = await test.Accounts.Onboard("Unrelated").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage mismatched = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(source).To(destination, 9m).CreateRequest());
        TransactionHandle transaction = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 16)).For(10m).From(source).To(destination).CreateAsync();
        using HttpResponseMessage wrongAccount = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = unrelated.Id,
            Date = new DateOnly(2026, 7, 16)
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, mismatched.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, wrongAccount.StatusCode);
        CollectionModel<TransactionModel> transactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>("/transactions");
        Assert.Equal(transaction.Id, Assert.Single(transactions.Items).Id);
    }

    /// <summary>
    /// Rejects spending that assigns the same fund twice within one destination.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsDuplicateSpendingFundAssignments()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        using HttpResponseMessage response = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateSpendingTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Invalid spending",
            Amount = 10m,
            Source = new CreateSpendingTransactionSourceModel { AccountId = cash.Id },
            Destinations = [new CreateSpendingTransactionDestinationModel
            {
                Location = new LocationInputModel { NewLocationName = "Market" },
                Amount = 10m,
                FundAssignments = [
                    new CreateFundAmountModel { FundId = groceries.Id, Amount = 5m },
                    new CreateFundAmountModel { FundId = groceries.Id, Amount = 5m }
                ]
            }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Rejects income whose lines and destination allocation do not reconcile to its total.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsUnreconciledIncomeStructure()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();

        using HttpResponseMessage response = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Pay",
            Amount = 100m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = new LocationInputModel { NewLocationName = "Employer" },
                IncomeLines = [new CreateIncomeLineModel { Description = "Gross", Amount = 90m }],
                IncomeDeductions = []
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 100m,
                FundAssignments = [new CreateIncomeFundAmountModel { FundId = income.Id, Amount = 90m }]
            }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Rejects extra funding when the selected fund goal has no planned monthly contribution.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsExtraContributionWithoutPlannedFundGoalContribution()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle gifts = await test.Funds.Create("Gifts").In(july).CreateAsync();

        using HttpResponseMessage response = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Birthday gift",
            Amount = 50m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = new LocationInputModel { NewLocationName = "Family" },
                IncomeLines = [new CreateIncomeLineModel { Description = "Gift", Amount = 50m }],
                IncomeDeductions = [],
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 50m,
                FundAssignments = [new CreateIncomeFundAmountModel { FundId = gifts.Id, Amount = 50m, IsExtraContribution = true }],
            }],
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Allows income to be split between tracked and untracked accounts, while requiring at least one tracked destination.
    /// </summary>
    [Fact]
    public async Task CreateAsyncAllowsUntrackedIncomeDestinationsAlongsideTrackedDestination()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle debt = await test.Accounts.Create("Debt", july).WithType(AccountTypeModel.Debt).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();

        _ = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(100m)
            .From("Employer").To(cash, income, 60m).To(debt, null, 40m).CreateAsync();
        using HttpResponseMessage onlyUntracked = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From("Employer").To(debt, null).CreateRequest());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, onlyUntracked.StatusCode);
    }

    /// <summary>
    /// Rejects transfers that use the same source and destination resource.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsSelfReferencingAccountAndFundTransfers()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        using HttpResponseMessage accountResponse = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To(cash).CreateRequest());
        using HttpResponseMessage fundResponse = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(groceries).To(groceries).CreateRequest());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, accountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundResponse.StatusCode);
    }

    /// <summary>
    /// Rejects request resources that cannot be resolved and update models that do not match the stored type.
    /// </summary>
    [Fact]
    public async Task RequestConversionRejectsUnknownResourcesAndMismatchedUpdateTypes()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        AccountHandle unknownAccount = new(Guid.NewGuid(), "Unknown");
        FundHandle unknownFund = new(Guid.NewGuid(), "Unknown", new FundGoalHandle(Guid.NewGuid(), Guid.NewGuid()));

        using HttpResponseMessage unknownAccountResponse = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(unknownAccount).To("Market", groceries).CreateRequest());
        using HttpResponseMessage unknownFundResponse = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 16)).For(10m).From(unknownFund).To(groceries).CreateRequest());
        using HttpResponseMessage mismatchedType = await test.Api.PostResponseAsync($"/transactions/{spending.Id}",
            test.Transactions.Fund().On(new DateOnly(2026, 7, 15)).For(10m).From(groceries).To(groceries).UpdateRequest());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownAccountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownFundResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, mismatchedType.StatusCode);
    }

    /// <summary>
    /// Rejects accounts and funds whose opening period is later than the transaction period.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsResourcesThatDidNotExistInTheTransactionPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        AccountHandle laterAccount = await test.Accounts.Create("Later account", august).CreateAsync();
        FundHandle laterFund = await test.Funds.Create("Later fund").In(august).CreateAsync();

        using HttpResponseMessage accountResponse = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To(laterAccount).CreateRequest());
        using HttpResponseMessage fundResponse = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", laterFund).CreateRequest());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, accountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundResponse.StatusCode);
    }

    /// <summary>
    /// Rejects updating or deleting a posted transaction even when the request itself is otherwise valid.
    /// </summary>
    [Fact]
    public async Task UpdateAndDeleteAsyncRejectPostedTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        SpendingTransactionBuilder spending = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries);
        TransactionHandle transaction = await spending.CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));

        using HttpResponseMessage update = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}", spending.UpdateRequest());
        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/transactions/{transaction.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, update.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
    }

    /// <summary>
    /// Rejects duplicate posts, dates before the transaction, and dates outside the accounting-period posting window.
    /// </summary>
    [Fact]
    public async Task PostAsyncEnforcesTransactionDateAndPostingWindow()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        using HttpResponseMessage beforeTransaction = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 7, 14)
        });
        using HttpResponseMessage outsideWindow = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 9, 1)
        });
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        using HttpResponseMessage duplicate = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 7, 16)
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, beforeTransaction.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, outsideWindow.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);
    }

    /// <summary>
    /// Rejects unposting a pending or missing transaction and posting or unposting a fund transfer.
    /// </summary>
    [Fact]
    public async Task PostAndUnpostAsyncRejectInvalidTransactionStates()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        TransactionHandle transfer = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(groceries).To(dining).CreateAsync();

        using HttpResponseMessage pendingUnpost = await test.Api.PostResponseAsync($"/transactions/{spending.Id}/unpost", new { });
        using HttpResponseMessage missingUnpost = await test.Api.PostResponseAsync($"/transactions/{Guid.NewGuid()}/unpost", new { });
        using HttpResponseMessage fundPost = await test.Api.PostResponseAsync($"/transactions/{transfer.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 7, 15)
        });
        using HttpResponseMessage fundUnpost = await test.Api.PostResponseAsync($"/transactions/{transfer.Id}/unpost", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, pendingUnpost.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingUnpost.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundPost.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundUnpost.StatusCode);
    }

    /// <summary>
    /// Rejects unposting and deletion once a transaction's accounting period is closed.
    /// </summary>
    [Fact]
    public async Task ClosedPeriodRejectsTransactionMutations()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        await test.Periods.CloseAsync(july);

        using HttpResponseMessage unpost = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/unpost", new { });
        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/transactions/{transaction.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, unpost.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
    }
}
