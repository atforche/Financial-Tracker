using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
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
    [ClassData(typeof(BalanceEventDateScenarios))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new BalanceEventDateScenarioSetup();
        if (ShouldThrowException(setup, eventDate))
        {
            Assert.Throws<InvalidOperationException>(() => AddChangeInValue(setup, eventDate));
            return;
        }
        new ChangeInValueValidator().Validate(AddChangeInValue(setup, eventDate), GetExpectedState(setup, eventDate));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(BalanceEventDateScenarioSetup setup, DateOnly eventDate) =>
        setup.CalculateMonthDifference(eventDate) > 1;

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(BalanceEventDateScenarioSetup setup, DateOnly eventDate) =>
        setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.CurrentAccountingPeriod,
            eventDate,
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
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static ChangeInValueState GetExpectedState(BalanceEventDateScenarioSetup setup, DateOnly eventDate) =>
        new()
        {
            AccountName = setup.Account.Name,
            EventDate = eventDate,
            EventSequence = 1,
            AccountingEntry = new FundAmountState
            {
                FundName = setup.Fund.Name,
                Amount = -100.00m,
            }
        };
}