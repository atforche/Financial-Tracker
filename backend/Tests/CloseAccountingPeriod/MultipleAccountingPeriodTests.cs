using Domain.AccountingPeriods;
using Domain.Actions;
using Tests.CloseAccountingPeriod.Scenarios;
using Tests.CloseAccountingPeriod.Setups;
using Tests.Validators;

namespace Tests.CloseAccountingPeriod;

/// <summary>
/// Test class that tests closing an Accounting Period with different <see cref="MultipleAccountingPeriodScenarios"/>
/// </summary>
public class MultipleAccountingPeriodTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(MultipleAccountingPeriodScenarios))]
    public void RunTest(DateOnly firstPeriod, DateOnly secondPeriod, DateOnly thirdPeriod)
    {
        using var setup = new MultipleAccountingPeriodScenarioSetup();
        if (!MultipleAccountingPeriodScenarios.IsValid(firstPeriod, secondPeriod, thirdPeriod))
        {
            Assert.Throws<InvalidOperationException>(() => RunTestPrivate(setup, firstPeriod, secondPeriod, thirdPeriod));
            return;
        }
        RunTestPrivate(setup, firstPeriod, secondPeriod, thirdPeriod);
    }

    /// <summary>
    /// Runs the bulk of this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="firstPeriod">First Period for this test case</param>
    /// <param name="secondPeriod">Second period for this test case</param>
    /// <param name="thirdPeriod">Third period for this test case</param>
    private static void RunTestPrivate(
        MultipleAccountingPeriodScenarioSetup setup,
        DateOnly firstPeriod,
        DateOnly secondPeriod,
        DateOnly thirdPeriod)
    {
        List<DateOnly> periodsToClose = [firstPeriod, secondPeriod, thirdPeriod];
        foreach (DateOnly periodToClose in periodsToClose)
        {
            setup.GetService<CloseAccountingPeriodAction>().Run(
                setup.AccountingPeriods.First(period => period.Year == periodToClose.Year && period.Month == periodToClose.Month));
            new AccountingPeriodValidator().Validate(
                setup.AccountingPeriods.ElementAt(0),
                GetExpectedState(setup, setup.AccountingPeriods.ElementAt(0)));
            new AccountingPeriodValidator().Validate(
                setup.AccountingPeriods.ElementAt(1),
                GetExpectedState(setup, setup.AccountingPeriods.ElementAt(1)));
            new AccountingPeriodValidator().Validate(
                setup.AccountingPeriods.ElementAt(2),
                GetExpectedState(setup, setup.AccountingPeriods.ElementAt(2)));
            new AccountBalanceCheckpointValidator().Validate(
                setup.Account.AccountBalanceCheckpoints,
                GetExpectedAccountBalanceCheckpointStates(setup));
        }
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="period">Accounting Period for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountingPeriodState GetExpectedState(MultipleAccountingPeriodScenarioSetup setup, AccountingPeriod period)
    {
        List<AccountAddedBalanceEventState> expectedBalanceEvents = [];
        if (period == setup.AccountingPeriods.First())
        {
            expectedBalanceEvents.Add(new AccountAddedBalanceEventState
            {
                AccountingPeriodKey = period.Key,
                AccountName = setup.Account.Name,
                EventDate = period.PeriodStartDate,
                EventSequence = 1,
                FundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m,
                    }
                ]
            });
        }
        return new()
        {
            Key = period.Key,
            IsOpen = period.IsOpen,
            Transactions = [],
            ChangeInValues = [],
            FundConversions = [],
            AccountAddedBalanceEvents = expectedBalanceEvents
        };
    }

    /// <summary>
    /// Gets the expected Account Balance Checkpoint States for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected Account Balance Checkpoint States for this test case</returns>
    private static List<AccountBalanceCheckpointState> GetExpectedAccountBalanceCheckpointStates(MultipleAccountingPeriodScenarioSetup setup)
    {
        List<AccountBalanceCheckpointState> results = [];
        foreach (AccountingPeriod accountingPeriod in setup.AccountingPeriods.OrderBy(period => period.PeriodStartDate))
        {
            AccountingPeriod? futureAccountingPeriod = setup.AccountingPeriods
                .FirstOrDefault(period => period.PeriodStartDate == accountingPeriod.PeriodStartDate.AddMonths(1));
            if (!accountingPeriod.IsOpen && futureAccountingPeriod != null)
            {
                results.Add(new AccountBalanceCheckpointState
                {
                    AccountName = setup.Account.Name,
                    AccountingPeriodId = futureAccountingPeriod.Id,
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
        }
        return results.ToList();
    }
}