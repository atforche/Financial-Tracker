using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;
using Models.Transactions.Update;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers income deductions through the public transaction contract.
/// </summary>
public sealed class IncomeDeductionTransactionTests
{
    /// <summary>
    /// Persists deductions through create, update, read, and posting operations.
    /// </summary>
    [Fact]
    public async Task IncomeDeductionsReconcileAndProjectAcrossReadAndBalanceSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();

        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Paycheck",
            Amount = 100m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = "Employer",
                Income = IncomeBreakdownModelFactory.Payroll(
                    [("Salary", 120m), ("Bonus", 30m)],
                    [("Tax", 50m)])
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 100m,
                FundAssignments = [new CreateIncomeFundAmountModel { FundId = income.Id, Amount = 100m }]
            }]
        });
        TransactionHandle transaction = new(created.Id);

        UpdateTransactionModel update = new UpdateIncomeTransactionModel
        {
            Date = new DateOnly(2026, 7, 16),
            Description = "Updated paycheck",
            Amount = 150m,
            Source = new UpdateIncomeTransactionSourceModel
            {
                Location = "Employer",
                Income = IncomeBreakdownModelFactory.Payroll(
                    [("Salary", 200m)],
                    [("Tax", 40m), ("Benefits", 10m)])
            },
            Destinations = [new UpdateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 150m,
                FundAssignments = [new CreateIncomeFundAmountModel { FundId = income.Id, Amount = 150m }]
            }]
        };
        await test.Api.PostAsync($"/transactions/{transaction.Id}", update);

        IncomeTransactionModel detail = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{transaction.Id}");
        Assert.Equal(150m, detail.Amount);
        Assert.Equal(150m, detail.TrackedAmount);
        Assert.Equal(["Tax", "Benefits"], detail.Source.Income.EmployeeDeductions.Select(deduction => deduction.Description));
        Assert.Equal(50m, detail.Source.Income.EmployeeDeductions.Sum(deduction => deduction.Amount));

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        AccountBalanceSnapshot account = await test.AccountQueries.GetBalanceAsync(cash);
        FundBalanceSnapshot fund = await test.FundQueries.GetBalanceAsync(income);
        FundGoalAvailabilitySnapshot goal = await test.FundGoalQueries.GetAvailabilityAsync(income.Goal);
        Assert.Equal(250m, account.Posted);
        Assert.Equal(150m, fund.Posted);
        Assert.Equal(150m, goal.Posted);
    }
}