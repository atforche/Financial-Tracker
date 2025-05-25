using Domain.AccountingPeriods;
using Domain.Actions;
using Tests.AddFundConversion.Scenarios;
using Tests.AddFundConversion.Setups;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different <see cref="MultipleBalanceEventScenarios"/>
/// </summary>
public class MultipleBalanceEventTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(MultipleBalanceEventScenarios))]
    public void RunTest(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        using var setup = new MultipleBalanceEventScenarioSetup(scenario);
        if (!MultipleBalanceEventScenarios.IsValid(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => AddFundConversion(setup));
            return;
        }
        new FundConversionValidator().Validate(AddFundConversion(setup), GetExpectedState(setup, scenario));
    }

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(MultipleBalanceEventScenarioSetup setup) =>
        setup.GetService<AddFundConversionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            500.00m);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(
        MultipleBalanceEventScenarioSetup setup,
        AddBalanceEventMultipleBalanceEventScenario scenario) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = scenario == AddBalanceEventMultipleBalanceEventScenario.MultipleEventsSameDay ? 2 : 1,
            AccountName = setup.Account.Name,
            FromFundName = setup.Fund.Name,
            ToFundName = setup.OtherFund.Name,
            Amount = 500.00m
        };
}