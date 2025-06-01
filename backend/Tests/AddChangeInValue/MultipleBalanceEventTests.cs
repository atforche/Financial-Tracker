using Domain.AccountingPeriods;
using Domain.Funds;
using Tests.AddChangeInValue.Setups;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with different <see cref="AddBalanceEventMultipleBalanceEventScenarios"/>
/// </summary>
public class MultipleBalanceEventTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventMultipleBalanceEventScenarios))]
    public void RunTest(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        using var setup = new MultipleBalanceEventScenarioSetup(scenario);
        if (!IsValid(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => AddChangeInValue(setup));
            return;
        }
        new ChangeInValueValidator().Validate(AddChangeInValue(setup), GetExpectedState(setup, scenario));
    }

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    private static bool IsValid(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        List<AddBalanceEventMultipleBalanceEventScenario> invalidScenarios =
        [
            AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceNegative,
            AddBalanceEventMultipleBalanceEventScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative,
        ];
        return !invalidScenarios.Contains(scenario);
    }

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(MultipleBalanceEventScenarioSetup setup) =>
        setup.GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            AccountId = setup.Account.Id,
            FundAmount = new FundAmount
            {
                FundId = setup.Fund.Id,
                Amount = -500.00m,
            }
        });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static ChangeInValueState GetExpectedState(
        MultipleBalanceEventScenarioSetup setup,
        AddBalanceEventMultipleBalanceEventScenario scenario) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = scenario == AddBalanceEventMultipleBalanceEventScenario.MultipleEventsSameDay ? 2 : 1,
            AccountId = setup.Account.Id,
            FundAmount = new FundAmountState
            {
                FundId = setup.Fund.Id,
                Amount = -500.00m,
            }
        };
}