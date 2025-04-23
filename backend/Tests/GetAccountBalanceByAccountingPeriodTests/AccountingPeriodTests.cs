using Domain.Services;
using Domain.ValueObjects;
using Tests.GetAccountBalanceByAccountingPeriodTests.Scenarios;
using Tests.GetAccountBalanceByAccountingPeriodTests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByAccountingPeriodTests;

/// <summary>
/// Test class that tests getting an Account Balance by Accounting Period with different <see cref="AccountingPeriodScenarios"/>
/// </summary>
public class AccountingPeriodTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountingPeriodScenarios))]
    public void RunTest(AccountingPeriodScenario scenario)
    {
        var setup = new AccountingPeriodScenarioSetup(scenario);
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Gets the Account Balance by Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Accounting Period for this test case</returns>
    private static AccountBalanceByAccountingPeriod GetAccountBalance(AccountingPeriodScenarioSetup setup) =>
        setup.GetService<IAccountBalanceService>().GetAccountBalancesByAccountingPeriod(setup.Account, setup.AccountingPeriod);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountBalanceByAccountingPeriodState GetExpectedState(AccountingPeriodScenarioSetup setup)
    {
        List<FundAmountState> expectedFundAmounts = setup.AccountingPeriod.Month == 12
            ? []
            : [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 1500.00m
                }
            ];
        return new AccountBalanceByAccountingPeriodState
        {
            AccountingPeriodYear = setup.AccountingPeriod.Year,
            AccountingPeriodMonth = setup.AccountingPeriod.Month,
            StartingFundBalances = expectedFundAmounts,
            EndingFundBalances = expectedFundAmounts,
            EndingPendingFundBalanceChanges = []
        };
    }
}