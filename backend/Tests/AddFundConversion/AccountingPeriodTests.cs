using Domain.AccountingPeriods;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different <see cref="AddBalanceEventAccountingPeriodScenarios"/>
/// </summary>
public class AccountingPeriodTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventAccountingPeriodScenarios))]
    public void RunTest(AddBalanceEventAccountingPeriodScenario scenario)
    {
        using var setup = new AddBalanceEventAccountingPeriodScenarioSetup(scenario);
        if (!AddBalanceEventAccountingPeriodScenarios.IsValid(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => AddFundConversion(setup));
            return;
        }
        new FundConversionValidator().Validate(AddFundConversion(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(AddBalanceEventAccountingPeriodScenarioSetup setup) =>
        setup.GetService<FundConversionFactory>().Create(new CreateFundConversionRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            AccountId = setup.Account.Id,
            FromFundId = setup.Fund.Id,
            ToFundId = setup.OtherFund.Id,
            Amount = 100.00m
        });

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(AddBalanceEventAccountingPeriodScenarioSetup setup) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = 1,
            AccountId = setup.Account.Id,
            FromFundId = setup.Fund.Id,
            ToFundId = setup.OtherFund.Id,
            Amount = 100.00m,
        };
}