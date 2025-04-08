using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

#pragma warning disable CA1724
/// <summary>
/// Test class that tests adding an Accounting Period with different Accounting Period scenarios
/// </summary>
public class AccountingPeriodTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddAccountingPeriodScenarios))]
    public void RunTest(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        if (ShouldThrowException(pastPeriodStatus, currentPeriodStatus, futurePeriodStatus))
        {
            Assert.Throws<InvalidOperationException>(() =>
                new AccountingPeriodScenarioSetup(pastPeriodStatus, currentPeriodStatus, futurePeriodStatus));
            return;
        }
        var setup = new AccountingPeriodScenarioSetup(pastPeriodStatus, currentPeriodStatus, futurePeriodStatus);
        if (setup.PastAccountingPeriod != null)
        {
            new AccountingPeriodValidator().Validate(setup.PastAccountingPeriod, GetExpectedState(setup, setup.PastAccountingPeriod));
        }
        new AccountingPeriodValidator().Validate(setup.CurrentAccountingPeriod, GetExpectedState(setup, setup.CurrentAccountingPeriod));
        if (setup.FutureAccountingPeriod != null)
        {
            new AccountingPeriodValidator().Validate(setup.FutureAccountingPeriod, GetExpectedState(setup, setup.FutureAccountingPeriod));
        }

        // Verify that adding a future Accounting Period with a gap always fails
        Assert.Throws<InvalidOperationException>(() => AddAccountingPeriod(setup, 2025, 12));

        // Verify that adding an Accounting Period in the past always fails
        Assert.Throws<InvalidOperationException>(() => AddAccountingPeriod(setup, 2024, setup.PastAccountingPeriod != null ? 11 : 12));

        // Verify that adding a duplicate Accounting Period always fails
        Assert.Throws<InvalidOperationException>(() => AddAccountingPeriod(setup, 2025, 1));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="pastPeriodStatus">Past period status for this test case</param>
    /// <param name="currentPeriodStatus">Current period status for this test case</param>
    /// <param name="futurePeriodStatus">Future period status for this test case</param>
    /// <returns></returns>
    private static bool ShouldThrowException(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        if (futurePeriodStatus == AccountingPeriodStatus.Closed)
        {
            if (currentPeriodStatus == AccountingPeriodStatus.Open || pastPeriodStatus == AccountingPeriodStatus.Open)
            {
                return true;
            }
        }
        return currentPeriodStatus == AccountingPeriodStatus.Closed && pastPeriodStatus == AccountingPeriodStatus.Open;
    }

    /// <summary>
    /// Adds the Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <returns>The Accounting Period for this test case</returns>
    private static AccountingPeriod AddAccountingPeriod(AccountingPeriodScenarioSetup setup, int year, int month) =>
        setup.GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(year, month);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriod">Accounting Period to get the expected state for</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountingPeriodState GetExpectedState(AccountingPeriodScenarioSetup setup, AccountingPeriod accountingPeriod)
    {
        List<AccountBalanceCheckpointState> expectedAccountBalanceCheckpoints = [];
        if (accountingPeriod == setup.PastAccountingPeriod ||
            (accountingPeriod == setup.CurrentAccountingPeriod && (!setup.PastAccountingPeriod?.IsOpen ?? true)) ||
            (accountingPeriod == setup.FutureAccountingPeriod &&
                (!setup.PastAccountingPeriod?.IsOpen ?? true) &&
                (!setup.CurrentAccountingPeriod?.IsOpen ?? true)))
        {
            expectedAccountBalanceCheckpoints.Add(new AccountBalanceCheckpointState
            {
                AccountName = setup.Account.Name,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1500.00m
                    },
                    new FundAmountState
                    {
                        FundName = setup.OtherFund.Name,
                        Amount = 1500.00m
                    }
                ]
            });
        }
        return new()
        {
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            IsOpen = accountingPeriod.IsOpen,
            AccountBalanceCheckpoints = expectedAccountBalanceCheckpoints,
            Transactions = []
        };
    }
}