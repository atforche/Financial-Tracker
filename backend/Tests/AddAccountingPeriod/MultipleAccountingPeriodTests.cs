using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.AddAccountingPeriod.Scenarios;
using Tests.AddAccountingPeriod.Setups;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding an Accounting Period with different Multiple Accounting Period scenarios
/// </summary>
public class MultipleAccountingPeriodTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(MultipleAccountingPeriodScenarios))]
    public void RunTest(
        DateOnly firstPeriod,
        DateOnly secondPeriod,
        DateOnly thirdPeriod,
        bool shouldClosePeriods)
    {
        if (!MultipleAccountingPeriodScenarios.IsValid(firstPeriod, secondPeriod, thirdPeriod))
        {
            Assert.Throws<InvalidOperationException>(() => RunTestPrivate(firstPeriod, secondPeriod, thirdPeriod, shouldClosePeriods));
            return;
        }
        RunTestPrivate(firstPeriod, secondPeriod, thirdPeriod, shouldClosePeriods);
    }

    /// <summary>
    /// Runs the bulk of this test case
    /// </summary>
    /// <param name="firstPeriod">First Period for this test case</param>
    /// <param name="secondPeriod">Second period for this test case</param>
    /// <param name="thirdPeriod">Third period for this test case</param>
    /// <param name="shouldClosePeriods">True if Accounting Periods should be closed before adding a new one, false otherwise</param>
    private static void RunTestPrivate(DateOnly firstPeriod, DateOnly secondPeriod, DateOnly thirdPeriod, bool shouldClosePeriods)
    {
        var setup = new MultipleAccountingPeriodScenarioSetup(firstPeriod, shouldClosePeriods);
        new AccountingPeriodValidator().Validate(setup.FirstAccountingPeriod, GetExpectedState(setup, setup.FirstAccountingPeriod));
        if (shouldClosePeriods)
        {
            setup.GetService<IAccountingPeriodService>().ClosePeriod(setup.FirstAccountingPeriod);
        }
        AccountingPeriod secondAccountingPeriod = setup.GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(secondPeriod.Year, secondPeriod.Month);
        setup.GetService<IAccountingPeriodRepository>().Add(secondAccountingPeriod);
        new AccountingPeriodValidator().Validate(secondAccountingPeriod, GetExpectedState(setup, secondAccountingPeriod));
        if (shouldClosePeriods)
        {
            setup.GetService<IAccountingPeriodService>().ClosePeriod(secondAccountingPeriod);
        }
        AccountingPeriod thirdAccountingPeriod = setup.GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(thirdPeriod.Year, thirdPeriod.Month);
        new AccountingPeriodValidator().Validate(thirdAccountingPeriod, GetExpectedState(setup, thirdAccountingPeriod));
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriod">Accounting Period to get the expected state for</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountingPeriodState GetExpectedState(MultipleAccountingPeriodScenarioSetup setup, AccountingPeriod accountingPeriod)
    {
        List<AccountBalanceCheckpointState> expectedAccountBalanceCheckpoints = [];
        if (accountingPeriod == setup.FirstAccountingPeriod || setup.ShouldClosePeriods)
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