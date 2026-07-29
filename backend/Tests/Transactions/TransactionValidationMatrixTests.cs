using System.Net;
using Models;
using Models.Accounts;
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
/// Covers validation at the transaction mutation boundary.
/// </summary>
public sealed class TransactionValidationMatrixTests
{
    /// <summary>
    /// Rejects an invalid destination total and posting through an unaffected Account.
    /// </summary>
    [Fact]
    public async Task MutationsRejectMismatchedAmountsAndUnaffectedPostingAccounts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle source = await test.Accounts.Onboard("Source").WithOpeningBalance(100m).CreateAsync();
        AccountHandle destination = await test.Accounts.Onboard("Destination").CreateAsync();
        AccountHandle unrelated = await test.Accounts.Onboard("Unrelated").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage mismatched = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateAccountTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Invalid transfer",
            Amount = 10m,
            Source = new CreateAccountTransactionSourceModel { AccountId = source.Id },
            Destinations = [new CreateAccountTransactionDestinationModel { AccountId = destination.Id, Amount = 9m }]
        });
        TransactionHandle transaction = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 16)).For(10m).From(source).To(destination).CreateAsync();
        using HttpResponseMessage wrongAccount = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = unrelated.Id,
            Date = new DateOnly(2026, 7, 16)
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, mismatched.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, wrongAccount.StatusCode);

        CollectionModel<TransactionModel> transactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>("/transactions");
        Assert.DoesNotContain(transactions.Items, item => item.Description == "Invalid transfer");
    }

    /// <summary>
    /// Rejects invalid spending destination and Fund assignment structures before they can create balance effects.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsInvalidSpendingDestinationsAndFundAssignments()
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
                Location = "Market",
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
    /// Rejects income whose lines, deductions, and destination allocation do not reconcile to its total.
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
                Location = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Gross", Amount = 90m }],
                IncomeDeductions = []
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 100m,
                FundAssignments = [new CreateFundAmountModel { FundId = income.Id, Amount = 90m }]
            }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Allows income to be split between tracked and untracked Accounts, while requiring at least one tracked destination.
    /// </summary>
    [Fact]
    public async Task CreateAsyncAllowsUntrackedIncomeDestinationsAlongsideTrackedDestination()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountModel debt = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Debt",
            Type = AccountTypeModel.Debt,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();

        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Split pay",
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
                    AccountId = cash.Id,
                    Amount = 60m,
                    FundAssignments = [new CreateFundAmountModel { FundId = income.Id, Amount = 60m }]
                },
                new CreateIncomeTransactionDestinationModel { AccountId = debt.Id, Amount = 40m, FundAssignments = [] }
            ]
        });
        using HttpResponseMessage onlyUntracked = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Debt payment",
            Amount = 100m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Pay", Amount = 100m }],
                IncomeDeductions = []
            },
            Destinations = [new CreateIncomeTransactionDestinationModel { AccountId = debt.Id, Amount = 100m, FundAssignments = [] }]
        });

        Assert.NotEqual(Guid.Empty, created.Id);
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

        using HttpResponseMessage accountResponse = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateAccountTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Self transfer",
            Amount = 10m,
            Source = new CreateAccountTransactionSourceModel { AccountId = cash.Id },
            Destinations = [new CreateAccountTransactionDestinationModel { AccountId = cash.Id, Amount = 10m }]
        });
        using HttpResponseMessage fundResponse = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateFundTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Self fund transfer",
            Amount = 10m,
            Source = new CreateFundTransactionSourceModel { FundId = groceries.Id },
            Destinations = [new CreateFundTransactionDestinationModel { FundId = groceries.Id, Amount = 10m }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, accountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundResponse.StatusCode);
    }

    /// <summary>
    /// Rejects updating a posted transaction even when the update request itself is otherwise valid.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncRejectsPostedTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));

        using HttpResponseMessage response = await test.Api.PostResponseAsync<UpdateTransactionModel>($"/transactions/{transaction.Id}", new UpdateSpendingTransactionModel
        {
            Date = new DateOnly(2026, 7, 15),
            Description = "Market",
            Amount = 10m,
            Source = new UpdateSpendingTransactionSourceModel { AccountId = cash.Id },
            Destinations = [new UpdateSpendingTransactionDestinationModel
            {
                Location = "Market",
                Amount = 10m,
                FundAssignments = [new CreateFundAmountModel { FundId = groceries.Id, Amount = 10m }]
            }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
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

        using HttpResponseMessage unknownFund = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateFundTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 16),
            Description = "Unknown fund",
            Amount = 10m,
            Source = new CreateFundTransactionSourceModel { FundId = Guid.NewGuid() },
            Destinations = [new CreateFundTransactionDestinationModel { FundId = groceries.Id, Amount = 10m }]
        });
        using HttpResponseMessage mismatchedType = await test.Api.PostResponseAsync<UpdateTransactionModel>($"/transactions/{spending.Id}", new UpdateFundTransactionModel
        {
            Date = new DateOnly(2026, 7, 15),
            Description = "Not spending",
            Amount = 10m,
            Source = new UpdateFundTransactionSourceModel { FundId = groceries.Id },
            Destinations = [new UpdateFundTransactionDestinationModel { FundId = groceries.Id, Amount = 10m }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownFund.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, mismatchedType.StatusCode);
    }

    /// <summary>
    /// Rejects Accounts and Funds whose opening period is later than the transaction period.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsResourcesThatDidNotExistInTheTransactionPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        AccountModel laterAccount = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Later account",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = august.Id,
            DateOpened = new DateOnly(2026, 8, 1)
        });
        FundHandle laterFund = await test.Funds.Create("Later fund").In(august).CreateAsync();

        using HttpResponseMessage accountResponse = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateAccountTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Early transfer",
            Amount = 10m,
            Source = new CreateAccountTransactionSourceModel { AccountId = cash.Id },
            Destinations = [new CreateAccountTransactionDestinationModel { AccountId = laterAccount.Id, Amount = 10m }]
        });
        using HttpResponseMessage fundResponse = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateSpendingTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Early spending",
            Amount = 10m,
            Source = new CreateSpendingTransactionSourceModel { AccountId = cash.Id },
            Destinations = [new CreateSpendingTransactionDestinationModel
            {
                Location = "Market",
                Amount = 10m,
                FundAssignments = [new CreateFundAmountModel { FundId = laterFund.Id, Amount = 10m }]
            }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, accountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundResponse.StatusCode);
    }
}