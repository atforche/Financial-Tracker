using Models;
using Models.Accounts;
using Models.Transactions;
using Models.Transactions.Types;

namespace Tests.Transactions;

/// <summary>
/// Covers transaction list and date-range filtering, sorting, paging, and typed detail contracts.
/// </summary>
public sealed class TransactionQueryTests
{
    /// <summary>
    /// Distinguishes fully posted transactions and orders them through the public query contract.
    /// </summary>
    [Fact]
    public async Task TransactionListSortsByFullyPostedAndReturnsTypedDetails()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        TransactionHandle pending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", await test.Funds.Create("Groceries").In(july).CreateAsync()).CreateAsync();
        TransactionHandle posted = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 16)).For(10m).From(cash).To(savings).CreateAsync();
        await test.Transactions.PostAsync(posted, cash, new DateOnly(2026, 7, 16));
        await test.Transactions.PostAsync(posted, savings, new DateOnly(2026, 7, 16));

        CollectionModel<TransactionModel> transactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            "/transactions?sort=FullyPostedDescending&limit=1");
        AccountTransactionModel detail = await test.Api.GetAsync<AccountTransactionModel>($"/transactions/{posted.Id}");

        TransactionModel first = Assert.Single(transactions.Items);
        Assert.Equal(posted.Id, first.Id);
        Assert.True(first.FullyPosted);
        _ = Assert.Single(detail.Destinations);
        Assert.Equal("Cash", detail.Source.Account!.Account.Name);
        Assert.Equal("Savings", Assert.Single(detail.Destinations).Account!.Account.Name);
        Assert.NotEqual(posted.Id, pending.Id);
    }

    /// <summary>
    /// Returns Transactions that affect a Location through either supported endpoint shape.
    /// </summary>
    [Fact]
    public async Task TransactionListFiltersByLocation()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 16)).For(100m).From("Employer").To(savings, groceries).CreateAsync();

        SpendingTransactionModel spendingDetail = await test.Api.GetAsync<SpendingTransactionModel>($"/transactions/{spending.Id}");
        IncomeTransactionModel incomeDetail = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{income.Id}");
        Guid marketId = Assert.Single(spendingDetail.Destinations).Location!.Id;
        Guid employerId = incomeDetail.Source.Location!.Id;

        CollectionModel<TransactionModel> marketTransactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.locationIds={marketId}");
        CollectionModel<TransactionModel> employerTransactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.locationIds={employerId}");

        Assert.Equal(spending.Id, Assert.Single(marketTransactions.Items).Id);
        Assert.Equal(income.Id, Assert.Single(employerTransactions.Items).Id);
    }

    /// <summary>
    /// Preserves every refund source and matches a refund through each source account.
    /// </summary>
    [Fact]
    public async Task RefundDetailsAndFiltersSupportMultipleSources()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle firstSource = await test.Accounts.Onboard("First Merchant Credit").WithType(AccountTypeModel.Investment).CreateAsync();
        AccountHandle secondSource = await test.Accounts.Onboard("Second Merchant Credit").WithType(AccountTypeModel.Investment).CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle refund = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 15)).For(25m)
            .From(firstSource, 10m).From(secondSource, 15m).To(cash, groceries).CreateAsync();

        RefundTransactionModel detail = await test.Api.GetAsync<RefundTransactionModel>($"/transactions/{refund.Id}");
        CollectionModel<TransactionModel> firstSourceTransactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.accountIds={firstSource.Id}");
        CollectionModel<TransactionModel> secondSourceTransactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.accountIds={secondSource.Id}");

        Assert.Equal(2, detail.Sources.Count);
        Assert.Equal(["First Merchant Credit", "Second Merchant Credit"], detail.Sources.Select(source => source.Account!.Account.Name));
        Assert.Equal(refund.Id, Assert.Single(firstSourceTransactions.Items).Id);
        Assert.Equal(refund.Id, Assert.Single(secondSourceTransactions.Items).Id);
    }

    /// <summary>
    /// Applies Account, Fund, period, and type filters before sorting the Transaction page.
    /// </summary>
    [Fact]
    public async Task TransactionListCombinesFiltersAndSortsByDestination()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle travel = await test.Funds.Create("Travel").In(july).CreateAsync();
        TransactionHandle groceriesTransaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 16)).For(30m).From(cash).To("Trip", travel).CreateAsync();
        _ = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 17)).For(10m).From(cash).To(savings).CreateAsync();

        CollectionModel<TransactionModel> result = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.accountIds={cash.Id}&filter.fundIds={groceries.Id}&filter.accountingPeriodIds={july.Id}&filter.types=Spending&sort=Destination");

        TransactionModel transaction = Assert.Single(result.Items);
        Assert.Equal(groceriesTransaction.Id, transaction.Id);
        Assert.Equal(1, result.TotalCount);
    }

    /// <summary>
    /// Applies date sorting and limits after filtering while retaining the unpaged total count.
    /// </summary>
    [Fact]
    public async Task TransactionListSortAndLimitPreserveFilteredTotalCount()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(10m).From(cash).To("First", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(10m).From(cash).To("Second", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 30)).For(10m).From(cash).To("Third", groceries).CreateAsync();

        CollectionModel<TransactionModel> response = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.accountIds={cash.Id}&filter.accountingPeriodIds={july.Id}&sort=DateDescending&limit=2");

        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(["Third", "Second"], response.Items.Select(item => item.Description));
    }

    /// <summary>
    /// Includes transactions on both date-range boundaries and excludes adjacent activity.
    /// </summary>
    [Fact]
    public async Task TransactionDateRangeIncludesBothBoundaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 1)).For(10m).From(cash).To("Start", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 31)).For(15m).From(cash).To("End", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 8, 1)).For(20m).From(cash).To("Outside", groceries).CreateAsync();

        TransactionsInDateRangeModel transactions = await test.Api.GetAsync<TransactionsInDateRangeModel>(
            "/transactions/date-range?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Equal(2, transactions.Transactions.TotalCount);
        Assert.Contains(transactions.Transactions.Items, item => item.Description == "Start");
        Assert.Contains(transactions.Transactions.Items, item => item.Description == "End");
        Assert.DoesNotContain(transactions.Transactions.Items, item => item.Description == "Outside");
    }

    /// <summary>
    /// Allows either date-range endpoint to constrain Transaction results.
    /// </summary>
    [Fact]
    public async Task TransactionDateRangeEndpointAllowsSingleEndpoints()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 1)).For(10m).From(cash).To("Early", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(15m).From(cash).To("Middle", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 31)).For(20m).From(cash).To("Late", groceries).CreateAsync();

        TransactionsInDateRangeModel throughMiddle = await test.Api.GetAsync<TransactionsInDateRangeModel>(
            "/transactions/date-range?range.end=2026-07-15");
        TransactionsInDateRangeModel fromMiddle = await test.Api.GetAsync<TransactionsInDateRangeModel>(
            "/transactions/date-range?range.start=2026-07-15");

        Assert.Equal(["Middle", "Early"], throughMiddle.Transactions.Items.Select(item => item.Description));
        Assert.Equal(["Late", "Middle"], fromMiddle.Transactions.Items.Select(item => item.Description));
    }
}
