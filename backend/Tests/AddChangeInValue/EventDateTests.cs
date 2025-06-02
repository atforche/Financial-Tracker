using Domain.ChangeInValues;
using Domain.Funds;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with different <see cref="AddBalanceEventDateScenarios"/>
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
        using var setup = new AddBalanceEventDateScenarioSetup(eventDate);
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
    private static ChangeInValue AddChangeInValue(AddBalanceEventDateScenarioSetup setup) =>
        setup.GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            EventDate = setup.EventDate,
            AccountId = setup.Account.Id,
            FundAmount = new FundAmount
            {
                FundId = setup.Fund.Id,
                Amount = -100.00m,
            }
        });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static ChangeInValueState GetExpectedState(AddBalanceEventDateScenarioSetup setup) =>
        new()
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            EventDate = setup.EventDate,
            EventSequence = 1,
            AccountId = setup.Account.Id,
            FundAmount = new FundAmountState
            {
                FundId = setup.Fund.Id,
                Amount = -100.00m,
            }
        };
}