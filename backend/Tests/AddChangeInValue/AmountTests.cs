using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with different Balance Event Amount scenarios
/// </summary>
public class AmountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventAmountScenarios))]
    public void RunTest(BalanceEventAmountScenario scenario)
    {
        var setup = new BalanceEventAmountScenarioSetup(scenario);
        if (ShouldThrowException(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => AddChangeInValue(setup));
            return;
        }
        new ChangeInValueValidator().Validate(AddChangeInValue(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(BalanceEventAmountScenario scenario)
    {
        List<BalanceEventAmountScenario> invalidScenarios =
        [
            BalanceEventAmountScenario.Zero,
            BalanceEventAmountScenario.ForcesAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative
        ];
        return invalidScenarios.Contains(scenario);
    }

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(BalanceEventAmountScenarioSetup setup) =>
        setup.GetService<AddChangeInValueAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 10),
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = setup.Amount,
            });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static ChangeInValueState GetExpectedState(BalanceEventAmountScenarioSetup setup) =>
        new()
        {
            AccountingPeriodKey = setup.AccountingPeriod.Key,
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = 1,
            AccountingEntry = new FundAmountState
            {
                FundName = setup.Fund.Name,
                Amount = setup.Amount,
            }
        };
}