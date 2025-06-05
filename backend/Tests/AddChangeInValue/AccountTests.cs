using Domain;
using Domain.Accounts;
using Domain.ChangeInValues;
using Domain.Funds;
using Tests.Mocks;
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
        var setup = new AccountScenarioSetup(accountType);
        new ChangeInValueValidator().Validate(AddChangeInValue(setup, 100.00m), GetExpectedState(setup, 100.00m));
        new ChangeInValueValidator().Validate(AddChangeInValue(setup, -100.00m), GetExpectedState(setup, -100.00m));
        new AccountBalanceByEventValidator().Validate(
            setup.GetService<AccountBalanceService>()
                .GetAccountBalancesByEvent(setup.Account.Id, new DateRange(new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 10))),
            [GetExpectedAccountBalance(setup, 100.00m), GetExpectedAccountBalance(setup, -100.00m)]);
    }

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this Change In Value</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(AccountScenarioSetup setup, decimal amount)
    {
        ChangeInValue changeInValue = setup.GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            AccountId = setup.Account.Id,
            FundAmount = new FundAmount
            {
                FundId = setup.Fund.Id,
                Amount = amount,
            }
        });
        setup.GetService<IChangeInValueRepository>().Add(changeInValue);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        return changeInValue;
    }

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
            AccountId = setup.Account.Id,
            FundAmount = new FundAmountState
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
            AccountId = setup.Account.Id,
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