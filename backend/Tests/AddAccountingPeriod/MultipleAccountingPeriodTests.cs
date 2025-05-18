using Domain.AccountingPeriods;
using Domain.Actions;
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
        using var setup = new MultipleAccountingPeriodScenarioSetup(firstPeriod);
        new AccountingPeriodValidator().Validate(setup.FirstAccountingPeriod, GetExpectedState(setup, setup.FirstAccountingPeriod));
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, GetExpectedAccountBalanceCheckpointStates(setup, null, null));
        if (shouldClosePeriods)
        {
            setup.GetService<CloseAccountingPeriodAction>().Run(setup.FirstAccountingPeriod);
        }

        AccountingPeriod secondAccountingPeriod = setup.GetService<AddAccountingPeriodAction>().Run(secondPeriod.Year, secondPeriod.Month);
        setup.GetService<IAccountingPeriodRepository>().Add(secondAccountingPeriod);
        new AccountingPeriodValidator().Validate(secondAccountingPeriod, GetExpectedState(setup, secondAccountingPeriod));
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, GetExpectedAccountBalanceCheckpointStates(setup, secondAccountingPeriod, null));
        if (shouldClosePeriods)
        {
            setup.GetService<CloseAccountingPeriodAction>().Run(secondAccountingPeriod);
        }

        AccountingPeriod thirdAccountingPeriod = setup.GetService<AddAccountingPeriodAction>().Run(thirdPeriod.Year, thirdPeriod.Month);
        new AccountingPeriodValidator().Validate(thirdAccountingPeriod, GetExpectedState(setup, thirdAccountingPeriod));
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, GetExpectedAccountBalanceCheckpointStates(setup, secondAccountingPeriod, thirdAccountingPeriod));
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriod">Accounting Period to get the expected state for</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountingPeriodState GetExpectedState(MultipleAccountingPeriodScenarioSetup setup, AccountingPeriod accountingPeriod)
    {
        List<AccountAddedBalanceEventState> expectedAccountAddedBalanceEvents = [];
        if (accountingPeriod == setup.FirstAccountingPeriod)
        {
            expectedAccountAddedBalanceEvents.Add(new AccountAddedBalanceEventState
            {
                AccountingPeriodKey = accountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = accountingPeriod.PeriodStartDate,
                EventSequence = 1,
                FundAmounts =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1500.00m
                    }
                ]
            });
        }
        return new AccountingPeriodState()
        {
            Key = accountingPeriod.Key,
            IsOpen = accountingPeriod.IsOpen,
            Transactions = [],
            FundConversions = [],
            ChangeInValues = [],
            AccountAddedBalanceEvents = expectedAccountAddedBalanceEvents
        };
    }

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
        List<AccountBalanceCheckpointState> results = [];
        if (!setup.FirstAccountingPeriod.IsOpen && secondAccountingPeriod != null)
        {
            results.Add(new AccountBalanceCheckpointState
            {
                AccountName = setup.Account.Name,
                AccountingPeriodKey = secondAccountingPeriod.Key,
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
                AccountingPeriodKey = thirdAccountingPeriod.Key,
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