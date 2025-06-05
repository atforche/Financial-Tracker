using Domain.FundConversions;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different <see cref="AddBalanceEventAmountScenarios"/>
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
        var setup = new AddBalanceEventAmountScenarioSetup();
        if (!IsValid(amount))
        {
            Assert.Throws<InvalidOperationException>(() => AddFundConversion(setup, amount));
            return;
        }
        new FundConversionValidator().Validate(AddFundConversion(setup, amount), GetExpectedState(setup, amount));
    }

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    private static bool IsValid(decimal amount) => amount > 0;

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        setup.GetService<FundConversionFactory>().Create(new CreateFundConversionRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            AccountId = setup.Account.Id,
            FromFundId = setup.Fund.Id,
            ToFundId = setup.OtherFund.Id,
            Amount = amount
        });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = 1,
            AccountId = setup.Account.Id,
            FromFundId = setup.Fund.Id,
            ToFundId = setup.OtherFund.Id,
            Amount = amount,
        };
}