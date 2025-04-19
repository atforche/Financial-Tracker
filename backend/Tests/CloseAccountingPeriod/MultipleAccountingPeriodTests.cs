using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
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
        var setup = new MultipleAccountingPeriodScenarioSetup();
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
            setup.GetService<IAccountingPeriodService>().ClosePeriod(
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
        List<AccountBalanceCheckpointState> expectedAccountBalanceCheckpoints = [];
        AccountingPeriod? previousAccountingPeriod = setup.AccountingPeriods
            .SingleOrDefault(otherPeriod => otherPeriod.Month == period.Month - 1);
        if (previousAccountingPeriod == null || !previousAccountingPeriod.IsOpen)
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
            Year = period.Year,
            Month = period.Month,
            IsOpen = period.IsOpen,
            AccountBalanceCheckpoints = expectedAccountBalanceCheckpoints,
            Transactions = []
        };
    }
}