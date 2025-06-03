using Domain.AccountingPeriods;
using Domain.Accounts;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByAccountingPeriodTests;

/// <summary>
/// Test class that tests getting an Account Balance by Accounting Period with different <see cref="GetAccountBalanceAccountingPeriodOverlapScenarios"/>
/// </summary>
public class AccountingPeriodOverlapTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(GetAccountBalanceAccountingPeriodOverlapScenarios))]
    public void RunTest(AccountingPeriodType accountingPeriodType, DateOnly eventDate)
    {
        using var setup = new GetAccountBalanceAccountingPeriodOverlapScenarioSetup(accountingPeriodType, eventDate);

        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.PastAccountingPeriod),
            GetPastPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.CurrentAccountingPeriod),
            GetCurrentPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.FutureAccountingPeriod),
            GetFuturePeriodExpectedState(setup, accountingPeriodType));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.PastAccountingPeriod);

        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.PastAccountingPeriod),
            GetPastPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.CurrentAccountingPeriod),
            GetCurrentPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.FutureAccountingPeriod),
            GetFuturePeriodExpectedState(setup, accountingPeriodType));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.CurrentAccountingPeriod);

        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.PastAccountingPeriod),
            GetPastPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.CurrentAccountingPeriod),
            GetCurrentPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.FutureAccountingPeriod),
            GetFuturePeriodExpectedState(setup, accountingPeriodType));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.FutureAccountingPeriod);

        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.PastAccountingPeriod),
            GetPastPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.CurrentAccountingPeriod),
            GetCurrentPeriodExpectedState(setup, accountingPeriodType));
        new AccountBalanceByAccountingPeriodValidator().Validate(GetAccountBalance(setup, setup.FutureAccountingPeriod),
            GetFuturePeriodExpectedState(setup, accountingPeriodType));
    }

    /// <summary>
    /// Gets the Account Balance by Accounting Period for the provided Accounting Period
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriod">Accounting Period to get the Account Balance by Accounting Period for</param>
    /// <returns>The Account Balance by Accounting Period for the provided Accounting Period</returns>
    private static AccountBalanceByAccountingPeriod GetAccountBalance(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriod accountingPeriod) =>
        setup.GetService<AccountBalanceService>().GetAccountBalanceByAccountingPeriod(setup.Account.Id, accountingPeriod.Id);

    /// <summary>
    /// Gets the expected state for the past Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <returns>The expected state for the past Accounting Period this test case</returns>
    private static AccountBalanceByAccountingPeriodState GetPastPeriodExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriodType accountingPeriodType) =>
        new()
        {
            AccountingPeriodId = setup.PastAccountingPeriod.Id,
            StartingFundBalances = [],
            EndingFundBalances = accountingPeriodType == AccountingPeriodType.Past
                ? [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1000.00m
                    }
                  ]
                : [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m
                    }
                  ],
            EndingPendingFundBalanceChanges = []
        };

    /// <summary>
    /// Gets the expected state for the current Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <returns>The expected state for the current Accounting Period for this test case</returns>
    private static AccountBalanceByAccountingPeriodState GetCurrentPeriodExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriodType accountingPeriodType) =>
        new()
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            StartingFundBalances = accountingPeriodType == AccountingPeriodType.Past
                ? [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1000.00m
                    }
                  ]
                : [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m
                    }
                  ],
            EndingFundBalances = accountingPeriodType == AccountingPeriodType.Past
                ? [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 750.00m
                    }
                  ]
                : [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1250.00m
                    }
                  ],
            EndingPendingFundBalanceChanges = []
        };

    /// <summary>
    /// Gets the expected state for the future Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <returns>The expected state for the future Accounting Period for this test case</returns>
    private static AccountBalanceByAccountingPeriodState GetFuturePeriodExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriodType accountingPeriodType) =>
        new()
        {
            AccountingPeriodId = setup.FutureAccountingPeriod.Id,
            StartingFundBalances = accountingPeriodType == AccountingPeriodType.Past
                ? [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 750.00m
                    }
                  ]
                : [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1250.00m
                    }
                  ],
            EndingFundBalances =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = 750.00m
                }
            ],
            EndingPendingFundBalanceChanges = []
        };
}