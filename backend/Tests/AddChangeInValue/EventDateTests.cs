using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with different Balance Event Date scenarios
/// </summary>
public class EventDateTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventDateScenarios))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new BalanceEventDateScenarioSetup(eventDate);
        if (!AddBalanceEventDateScenarios.IsValid(eventDate))
        {
            Assert.Throws<InvalidOperationException>(() => AddChangeInValue(setup));
            return;
        }
        new ChangeInValueValidator().Validate(AddChangeInValue(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(BalanceEventDateScenarioSetup setup) =>
        setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.CurrentAccountingPeriod,
            setup.EventDate,
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = -100.00m,
            });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static ChangeInValueState GetExpectedState(BalanceEventDateScenarioSetup setup) =>
        new()
        {
            AccountName = setup.Account.Name,
            EventDate = setup.EventDate,
            EventSequence = 1,
            AccountingEntry = new FundAmountState
            {
                FundName = setup.Fund.Name,
                Amount = -100.00m,
            }
        };
}