using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.GetAccountBalanceByAccountingPeriodTests.Scenarios;
using Tests.GetAccountBalanceByAccountingPeriodTests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByAccountingPeriodTests;

/// <summary>
/// Test class that tests getting an Account Balance by Accounting Period with different <see cref="AccountingPeriodOverlapScenarios"/>
/// </summary>
public class AccountingPeriodOverlapTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountingPeriodOverlapScenarios))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new AccountingPeriodOverlapScenarioSetup(eventDate);

        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.PastAccountingPeriod),
            GetExpectedState(setup, setup.PastAccountingPeriod));
        setup.GetService<CloseAccountingPeriodAction>().Run(setup.PastAccountingPeriod);
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.PastAccountingPeriod),
            GetExpectedState(setup, setup.PastAccountingPeriod));

        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.CurrentAccountingPeriod),
            GetExpectedState(setup, setup.CurrentAccountingPeriod));
        setup.GetService<CloseAccountingPeriodAction>().Run(setup.CurrentAccountingPeriod);
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.CurrentAccountingPeriod),
            GetExpectedState(setup, setup.CurrentAccountingPeriod));

        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.FutureAccountingPeriod),
            GetExpectedState(setup, setup.FutureAccountingPeriod));
        setup.GetService<CloseAccountingPeriodAction>().Run(setup.FutureAccountingPeriod);
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.FutureAccountingPeriod),
            GetExpectedState(setup, setup.FutureAccountingPeriod));
    }

    /// <summary>
    /// Gets the Account Balance by Accounting Period for the provided Accounting Period
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriod">Accounting Period to get the Account Balance by Accounting Period for</param>
    /// <returns>The Account Balance by Accounting Period for the provided Accounting Period</returns>
    private static AccountBalanceByAccountingPeriod GetAccountBalance(
        AccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriod accountingPeriod) =>
        setup.GetService<IAccountBalanceService>().GetAccountBalancesByAccountingPeriod(setup.Account, accountingPeriod);

    /// <summary>
    /// Gets the expected state for this test case and Accounting Period
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriod">Accounting Period to get the expected state for</param>
    /// <returns>The expected state for this test case and Accounting Period</returns>
    private static AccountBalanceByAccountingPeriodState GetExpectedState(
        AccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriod accountingPeriod)
    {
        List<FundAmountState> expectedStartingFundBalances = [];
        if (accountingPeriod != setup.PastAccountingPeriod)
        {
            expectedStartingFundBalances.Add(new FundAmountState
            {
                FundName = setup.Fund.Name,
                Amount = accountingPeriod == setup.FutureAccountingPeriod
                    ? 1250.00m
                    : 1500.00m
            });
        }
        return new AccountBalanceByAccountingPeriodState()
        {
            AccountingPeriodKey = accountingPeriod.Key,
            StartingFundBalances = expectedStartingFundBalances,
            EndingFundBalances =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = accountingPeriod == setup.CurrentAccountingPeriod || accountingPeriod == setup.FutureAccountingPeriod
                        ? 1250.00m
                        : 1500.00m
                }
            ],
            EndingPendingFundBalanceChanges = []
        };
    }
}