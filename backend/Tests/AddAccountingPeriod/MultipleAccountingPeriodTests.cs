using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.AddAccountingPeriod.Scenarios;
using Tests.AddAccountingPeriod.Setups;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding an Accounting Period with different <see cref="MultipleAccountingPeriodScenarios"/>
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
        var setup = new MultipleAccountingPeriodScenarioSetup(firstPeriod);
        new AccountingPeriodValidator().Validate(setup.FirstAccountingPeriod, GetExpectedState(setup.FirstAccountingPeriod));
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, GetExpectedAccountBalanceCheckpointStates(setup, null, null));
        if (shouldClosePeriods)
        {
            setup.GetService<IAccountingPeriodService>().ClosePeriod(setup.FirstAccountingPeriod);
        }

        AccountingPeriod secondAccountingPeriod = setup.GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(secondPeriod.Year, secondPeriod.Month);
        setup.GetService<IAccountingPeriodRepository>().Add(secondAccountingPeriod);
        new AccountingPeriodValidator().Validate(secondAccountingPeriod, GetExpectedState(secondAccountingPeriod));
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, GetExpectedAccountBalanceCheckpointStates(setup, secondAccountingPeriod, null));
        if (shouldClosePeriods)
        {
            setup.GetService<IAccountingPeriodService>().ClosePeriod(secondAccountingPeriod);
        }

        AccountingPeriod thirdAccountingPeriod = setup.GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(thirdPeriod.Year, thirdPeriod.Month);
        new AccountingPeriodValidator().Validate(thirdAccountingPeriod, GetExpectedState(thirdAccountingPeriod));
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, GetExpectedAccountBalanceCheckpointStates(setup, secondAccountingPeriod, thirdAccountingPeriod));
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to get the expected state for</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountingPeriodState GetExpectedState(AccountingPeriod accountingPeriod) =>
        new()
        {
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            IsOpen = accountingPeriod.IsOpen,
            Transactions = []
        };

    /// <summary>
    /// Gets the expected Account Balance Checkpoint States for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="secondAccountingPeriod">Second Accounting Period for this test case</param>
    /// <param name="thirdAccountingPeriod">Third Accounting Period for this test case</param>
    /// <returns>The expected Account Balance Checkpoint States for this test case</returns>
    private static List<AccountBalanceCheckpointState> GetExpectedAccountBalanceCheckpointStates(
        MultipleAccountingPeriodScenarioSetup setup,
        AccountingPeriod? secondAccountingPeriod,
        AccountingPeriod? thirdAccountingPeriod)
    {
        List<AccountBalanceCheckpointState> results =
        [
            new AccountBalanceCheckpointState
            {
                AccountName = setup.Account.Name,
                AccountingPeriodYear = setup.FirstAccountingPeriod.Year,
                AccountingPeriodMonth = setup.FirstAccountingPeriod.Month,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1500.00m
                    },
                ]
            }
        ];
        if (!setup.FirstAccountingPeriod.IsOpen && secondAccountingPeriod != null)
        {
            results.Add(new AccountBalanceCheckpointState
            {
                AccountName = setup.Account.Name,
                AccountingPeriodYear = secondAccountingPeriod.Year,
                AccountingPeriodMonth = secondAccountingPeriod.Month,
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
        if (secondAccountingPeriod?.IsOpen == false && thirdAccountingPeriod != null)
        {
            results.Add(new AccountBalanceCheckpointState
            {
                AccountName = setup.Account.Name,
                AccountingPeriodYear = thirdAccountingPeriod.Year,
                AccountingPeriodMonth = thirdAccountingPeriod.Month,
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
        return results;
    }
}