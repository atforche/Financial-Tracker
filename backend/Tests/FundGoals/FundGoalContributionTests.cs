using Models;
using Models.AccountingPeriods;
using Models.BalanceEvents;
using Models.FundGoals;
using Models.Funds;
using Models.Locations;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;

namespace Tests.FundGoals;

/// <summary>
/// Covers how transactions credit fund goals with planned and unplanned contributions.
/// </summary>
public sealed class FundGoalContributionTests
{
    /// <summary>
    /// Unplanned income funding increases ending-balance progress without satisfying the planned monthly contribution.
    /// </summary>
    [Fact]
    public async Task UnplannedIncomeFundingCountsTowardEndingBalanceButNotPlannedMonthlyContribution()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle gifts = await test.Funds.Create("Gifts").In(july).WithGoal(200m, 50m, 300m).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            ExpectedIncomeSourceId = null,
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 10),
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
                FundAssignments = [new CreateFundAmountModel { FundId = gifts.Id, Amount = 50m }],
            }],
        });

        AccountingPeriodWithBalanceModel pendingPeriod = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}");
        await test.Transactions.PostAsync(new TransactionHandle(created.Id), cash, new DateOnly(2026, 7, 10));
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{gifts.Goal.Id}/progress/{july.Id}");
        IncomeTransactionModel transaction = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{created.Id}");
        AccountingPeriodWithBalanceModel postedPeriod = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}");

        Assert.Equal(0m, pendingPeriod.ActualUnplannedGoalContributions);
        Assert.Equal(50m, postedPeriod.ActualUnplannedGoalContributions);
        Assert.Equal(0m, postedPeriod.ActualGoalContributions);
        Assert.Null(transaction.ExpectedIncomeSourceId);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(0m, progress.Contribution.AssignedAmount);
        Assert.Equal(200m, progress.Contribution.RemainingAmount);
        Assert.NotNull(progress.EndingBalance);
        Assert.Equal(50m, progress.EndingBalance.EndingBalance);
        Assert.Equal(FundGoalEndingBalanceStatusModel.WithinRange, progress.EndingBalance.Status);
    }

    /// <summary>
    /// Planning or unplanning income moves its assignments into or out of the planned monthly contribution, and deleting
    /// the Expected Income Source it was planned as leaves the contribution as it was posted.
    /// </summary>
    [Fact]
    public async Task PlanningIncomeMovesItsAssignmentsIntoThePlannedContribution()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).WithGoal(100m).CreateAsync();
        Guid employer = await test.Api.AddExpectedIncomeSourceAsync(july, "Employer", 100m);
        IncomeTransactionBuilder paycheck = test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(100m).From("Employer").To(cash, groceries);
        TransactionHandle income = await paycheck.CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));

        AccountingPeriodWithBalanceModel unplanned = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}");
        await test.Transactions.UnpostAsync(income);
        await paycheck.PlannedAs(employer).UpdateAsync(income);
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        AccountingPeriodWithBalanceModel planned = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}");
        await test.Api.DeleteAsync($"/accounting-periods/{july.Id}/expected-income-sources/{employer}");
        AccountingPeriodWithBalanceModel sourceDeleted = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}");
        IncomeTransactionModel transaction = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{income.Id}");

        Assert.Equal(0m, unplanned.ActualGoalContributions);
        Assert.Equal(100m, unplanned.ActualUnplannedGoalContributions);
        Assert.Equal(100m, unplanned.UnplannedIncome.Tracked);
        Assert.Equal(100m, planned.ActualGoalContributions);
        Assert.Equal(0m, planned.ActualUnplannedGoalContributions);
        Assert.Equal(0m, planned.UnplannedIncome.Total);
        Assert.Equal(100m, sourceDeleted.ActualGoalContributions);
        Assert.Equal(employer, transaction.ExpectedIncomeSourceId);
    }

    /// <summary>
    /// Assigned and planned monthly contribution totals remain distinct across income, spending, fund transfers,
    /// and lifecycle actions.
    /// </summary>
    [Fact]
    public async Task ContributionTotalsReconcileAcrossIncomeSpendingTransferAndLifecycleActions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).WithGoal(100m).CreateAsync();
        FundHandle reserve = await test.Funds.Create("Reserve").In(july).CreateAsync();
        Guid employer = await test.Api.AddExpectedIncomeSourceAsync(july, "Employer", 100m);
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(100m).From("Employer").PlannedAs(employer).To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 11)).For(30m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 12)).For(20m).From(groceries).To(reserve).CreateAsync();

        await AssertAssignedAsync(test, july, groceries, -20m);
        FundGoalBalanceEventModel transferEvent = Assert.Single(await GetGoalEventsAsync(test, july),
            item => item.Fund.Id == groceries.Id && item.Type == BalanceEventTypeModel.Debit && item.Source.DisplayName == groceries.Name);
        Assert.Equal(-20m, transferEvent.NewTotals?.AmountAssigned);
        Assert.Equal(-20m, transferEvent.NewTotals?.AmountAssignedToExpectedContribution);
        Assert.Equal(0m, transferEvent.NewTotals?.AmountSpent);

        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        await AssertAssignedAsync(test, july, groceries, 80m);

        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 11));
        await AssertAssignedAsync(test, july, groceries, 80m);
        FundGoalBalanceEventModel spendingEvent = Assert.Single(await GetGoalEventsAsync(test, july),
            item => item.Fund.Id == groceries.Id && item.Type == BalanceEventTypeModel.Debit && item.Source.DisplayName == "Cash");
        Assert.Equal(30m, spendingEvent.NewTotals?.AmountSpent);

        await test.Transactions.UnpostAsync(spending);
        await test.Transactions.UnpostAsync(income);
        await AssertAssignedAsync(test, july, groceries, -20m);

        await test.Transactions.DeleteAsync(spending);
        await test.Transactions.DeleteAsync(income);
        await AssertAssignedAsync(test, july, groceries, -20m);
    }

    /// <summary>
    /// Moving an automatic Unassigned income remainder into a configured fund goal moves contribution credit
    /// without changing the period aggregate.
    /// </summary>
    [Fact]
    public async Task UnassignedTopUpMovesContributionCreditWithoutCreatingAContribution()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle phone = await test.Funds.Create("Phone").In(july).WithGoal(105m).CreateAsync();
        FundHandle unassigned = await test.FundQueries.GetUnassignedAsync(july);
        Guid employer = await test.Api.AddExpectedIncomeSourceAsync(july, "Employer", 105m);
        CreateTransactionResultModel income = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            ExpectedIncomeSourceId = employer,
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 10),
            Description = "Pay",
            Amount = 105m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = new LocationInputModel { NewLocationName = "Employer" },
                IncomeLines = [new CreateIncomeLineModel { Description = "Pay", Amount = 105m }],
                IncomeDeductions = [],
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 105m,
                FundAssignments = [new CreateFundAmountModel { FundId = phone.Id, Amount = 100m }],
            }],
        });
        await test.Transactions.PostAsync(new TransactionHandle(income.Id), cash, new DateOnly(2026, 7, 10));

        await AssertContributionTotalAsync(test, july, unassigned, 5m);
        await AssertAssignedAsync(test, july, phone, 100m);
        Assert.Equal(105m, (await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}")).ActualGoalContributions);

        TransactionHandle transfer = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 11)).For(5m).From(unassigned).To(phone).CreateAsync();
        await AssertContributionTotalAsync(test, july, unassigned, 0m);
        await AssertAssignedAsync(test, july, phone, 105m);
        Assert.Equal(105m, (await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}")).ActualGoalContributions);

        await test.Transactions.DeleteAsync(transfer);
        await AssertContributionTotalAsync(test, july, unassigned, 5m);
        await AssertAssignedAsync(test, july, phone, 100m);
        Assert.Equal(105m, (await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{july.Id}")).ActualGoalContributions);
    }

    private static async Task AssertAssignedAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period, FundHandle fund, decimal assigned)
    {
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{fund.Goal.Id}/progress/{period.Id}");
        Assert.NotNull(progress.Contribution);
        Assert.Equal(assigned, progress.Contribution.AssignedAmount);
    }

    private static async Task AssertContributionTotalAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period, FundHandle fund, decimal expected)
    {
        FundGoalBalanceEventModel latest = (await GetGoalEventsAsync(test, period))
            .Where(item => item.Fund.Id == fund.Id)
            .OrderBy(item => item.TransactionDate)
            .ThenBy(item => item.TransactionSequence)
            .Last();
        Assert.Equal(expected, latest.NewTotals?.AmountAssignedToExpectedContribution);
    }

    private static async Task<IReadOnlyCollection<FundGoalBalanceEventModel>> GetGoalEventsAsync(FinancialTrackerTestContext test, AccountingPeriodHandle period) =>
        (await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={period.Id}&range.end={period.Id}")).Items;
}
