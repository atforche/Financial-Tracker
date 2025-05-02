using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different Balance Event Date scenarios
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
    private static FundConversion AddFundConversion(BalanceEventDateScenarioSetup setup) =>
        setup.GetService<AddFundConversionAction>().Run(setup.CurrentAccountingPeriod,
            setup.EventDate,
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            100.00m);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(BalanceEventDateScenarioSetup setup) =>
        new()
        {
            AccountingPeriodKey = setup.CurrentAccountingPeriod.Key,
            AccountName = setup.Account.Name,
            EventDate = setup.EventDate,
            EventSequence = 1,
            FromFundName = setup.Fund.Name,
            ToFundName = setup.OtherFund.Name,
            Amount = 100.00m,
        };
}