using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with different Accounting Period scenarios
/// </summary>
public class AccountingPeriodTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountingPeriodScenarios))]
    public void RunTest(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        var setup = new AccountingPeriodScenarioSetup(pastPeriodStatus, currentPeriodStatus, futurePeriodStatus);
        if (ShouldThrowException(setup))
        {
            Assert.Throws<InvalidOperationException>(() => AddChangeInValue(setup));
            return;
        }
        new ChangeInValueValidator().Validate(AddChangeInValue(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(AccountingPeriodScenarioSetup setup) => !setup.CurrentAccountingPeriod.IsOpen;

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(AccountingPeriodScenarioSetup setup) =>
        setup.GetService<AddChangeInValueAction>().Run(setup.CurrentAccountingPeriod,
            new DateOnly(2025, 1, 15),
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
    private static ChangeInValueState GetExpectedState(AccountingPeriodScenarioSetup setup) =>
        new()
        {
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = 1,
            AccountingEntry = new FundAmountState
            {
                FundName = setup.Fund.Name,
                Amount = -100.00m,
            }
        };
}