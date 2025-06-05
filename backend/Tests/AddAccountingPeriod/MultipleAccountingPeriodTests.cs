using Domain.AccountingPeriods;
using Tests.AddAccountingPeriod.Scenarios;
using Tests.AddAccountingPeriod.Setups;
using Tests.Mocks;
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
        new AccountValidator().Validate(setup.Account, GetExpectedAccountState(setup, null, null));
        if (shouldClosePeriods)
        {
            setup.GetService<CloseAccountingPeriodAction>().Run(setup.FirstAccountingPeriod);
        }
        setup.GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod secondAccountingPeriod = setup.GetService<AccountingPeriodFactory>().Create(secondPeriod.Year, secondPeriod.Month);
        setup.GetService<IAccountingPeriodRepository>().Add(secondAccountingPeriod);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        new AccountingPeriodValidator().Validate(secondAccountingPeriod, GetExpectedState(secondAccountingPeriod));
        new AccountValidator().Validate(setup.Account, GetExpectedAccountState(setup, secondAccountingPeriod, null));
        if (shouldClosePeriods)
        {
            setup.GetService<CloseAccountingPeriodAction>().Run(secondAccountingPeriod);
        }
        setup.GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod thirdAccountingPeriod = setup.GetService<AccountingPeriodFactory>().Create(thirdPeriod.Year, thirdPeriod.Month);
        new AccountingPeriodValidator().Validate(thirdAccountingPeriod, GetExpectedState(thirdAccountingPeriod));
        new AccountValidator().Validate(setup.Account, GetExpectedAccountState(setup, secondAccountingPeriod, thirdAccountingPeriod));
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
        };

    /// <summary>
    /// Gets the expected Account State for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="secondAccountingPeriod">Second Accounting Period for this test case</param>
    /// <param name="thirdAccountingPeriod">Third Accounting Period for this test case</param>
    /// <returns>The expected Account State for this test case</returns>
    private static AccountState GetExpectedAccountState(
        MultipleAccountingPeriodScenarioSetup setup,
        AccountingPeriod? secondAccountingPeriod,
        AccountingPeriod? thirdAccountingPeriod)
    {
        List<AccountBalanceCheckpointState> expectedAccountBalanceCheckpoints = [];
        if (!setup.FirstAccountingPeriod.IsOpen && secondAccountingPeriod != null)
        {
            expectedAccountBalanceCheckpoints.Add(new AccountBalanceCheckpointState
            {
                AccountName = setup.Account.Name,
                AccountingPeriodId = secondAccountingPeriod.Id,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m
                    },
                ]
            });
        }
        if (secondAccountingPeriod?.IsOpen == false && thirdAccountingPeriod != null)
        {
            expectedAccountBalanceCheckpoints.Add(new AccountBalanceCheckpointState
            {
                AccountName = setup.Account.Name,
                AccountingPeriodId = thirdAccountingPeriod.Id,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m
                    },
                ]
            });
        }
        return new AccountState
        {
            Name = setup.Account.Name,
            Type = setup.Account.Type,
            AccountAddedBalanceEvent = new AccountAddedBalanceEventState
            {
                AccountingPeriodId = setup.FirstAccountingPeriod.Id,
                EventDate = setup.FirstAccountingPeriod.PeriodStartDate,
                EventSequence = 1,
                AccountName = setup.Account.Name,
                FundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m
                    }
                ]
            },
            AccountBalanceCheckpoints = expectedAccountBalanceCheckpoints
        };
    }
}