using Domain.ChangeInValues;
using Domain.Funds;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with different <see cref="AddBalanceEventAmountScenarios"/>
/// </summary>
public class AmountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventAmountScenarios))]
    public void RunTest(decimal amount)
    {
        using var setup = new AddBalanceEventAmountScenarioSetup();
        if (!IsValid(amount))
        {
            Assert.Throws<InvalidOperationException>(() => AddChangeInValue(setup, amount));
            return;
        }
        new ChangeInValueValidator().Validate(AddChangeInValue(setup, amount), GetExpectedState(setup, amount));
    }

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    private static bool IsValid(decimal amount) => amount != 0.00m;

    /// <summary>
    /// Adds the Change In Value for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The Change In Value that was added for this test case</returns>
    private static ChangeInValue AddChangeInValue(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        setup.GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            AccountId = setup.Account.Id,
            FundAmount = new FundAmount
            {
                FundId = setup.Fund.Id,
                Amount = amount,
            }
        });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static ChangeInValueState GetExpectedState(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = 1,
            AccountId = setup.Account.Id,
            FundAmount = new FundAmountState
            {
                FundId = setup.Fund.Id,
                Amount = amount,
            }
        };
}