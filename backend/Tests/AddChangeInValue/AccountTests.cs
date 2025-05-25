using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Domain.Services;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with different <see cref="AccountScenarios"/>
/// </summary>
public class AccountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountScenarios))]
    public void RunTest(AccountType accountType)
    {
        using var setup = new AccountScenarioSetup(accountType);
        new ChangeInValueValidator().Validate(AddChangeInValue(setup, 100.00m), GetExpectedState(setup, 100.00m));
        new ChangeInValueValidator().Validate(AddChangeInValue(setup, -100.00m), GetExpectedState(setup, -100.00m));
        new AccountBalanceByEventValidator().Validate(
            setup.GetService<AccountBalanceService>()
                .GetAccountBalancesByEvent(setup.Account, new DateRange(new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 10))),
            [GetExpectedAccountBalance(setup, 100.00m), GetExpectedAccountBalance(setup, -100.00m)]);
    }

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this Change In Value</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(AccountScenarioSetup setup, decimal amount) =>
        setup.GetService<AddChangeInValueAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 10),
            setup.Account,
            new FundAmount
            {
                FundId = setup.Fund.Id,
                Amount = amount,
            });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this Change In Value</param>
    /// <returns>The expected state for this test case</returns>
    private static ChangeInValueState GetExpectedState(AccountScenarioSetup setup, decimal amount) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = amount < 0 ? 2 : 1,
            AccountName = setup.Account.Name,
            AccountingEntry = new FundAmountState
            {
                FundId = setup.Fund.Id,
                Amount = amount,
            }
        };

    /// <summary>
    /// Gets the expected Account Balance for this test case and Change In Value
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this Change In Value</param>
    /// <returns>The expected Account Balance for this test case and Change In Value</returns>
    private static AccountBalanceByEventState GetExpectedAccountBalance(AccountScenarioSetup setup, decimal amount) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = amount < 0 ? 2 : 1,
            AccountName = setup.Account.Name,
            FundBalances =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = amount < 0 ? 1500.00m : 1500.00m + amount,
                },
                new FundAmountState
                {
                    FundId = setup.OtherFund.Id,
                    Amount = 1500.00m,
                }
            ],
            PendingFundBalanceChanges = []
        };
}