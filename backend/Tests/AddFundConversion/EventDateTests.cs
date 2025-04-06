using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.Scenarios;
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
    [ClassData(typeof(BalanceEventDateScenarios))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new BalanceEventDateScenarioSetup(eventDate);
        if (ShouldThrowException(setup))
        {
            Assert.Throws<InvalidOperationException>(() => AddFundConversion(setup));
            return;
        }
        new FundConversionValidator().Validate(AddFundConversion(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(BalanceEventDateScenarioSetup setup) => setup.CalculateMonthDifference() > 1;

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(BalanceEventDateScenarioSetup setup) =>
        setup.GetService<IAccountingPeriodService>()
            .AddFundConversion(setup.CurrentAccountingPeriod,
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
            AccountName = setup.Account.Name,
            EventDate = setup.EventDate,
            EventSequence = 1,
            FromFundName = setup.Fund.Name,
            ToFundName = setup.OtherFund.Name,
            Amount = 100.00m,
        };
}