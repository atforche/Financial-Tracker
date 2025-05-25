using Domain.AccountingPeriods;
using Domain.Actions;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different <see cref="AddBalanceEventDateScenarios"/>
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
    private static FundConversion AddFundConversion(AddBalanceEventDateScenarioSetup setup) =>
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
    private static FundConversionState GetExpectedState(AddBalanceEventDateScenarioSetup setup) =>
        new()
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            EventDate = setup.EventDate,
            EventSequence = 1,
            AccountName = setup.Account.Name,
            FromFundName = setup.Fund.Name,
            ToFundName = setup.OtherFund.Name,
            Amount = 100.00m,
        };
}