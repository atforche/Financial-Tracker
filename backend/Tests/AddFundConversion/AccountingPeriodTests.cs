using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddFundConversion;

#pragma warning disable CA1724
/// <summary>
/// Test class that tests adding a Fund Conversion with different Accounting Period scenarios
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
    private static bool ShouldThrowException(AccountingPeriodScenarioSetup setup) => !setup.CurrentAccountingPeriod.IsOpen;

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(AccountingPeriodScenarioSetup setup) =>
        setup.GetService<IAccountingPeriodService>()
            .AddFundConversion(setup.CurrentAccountingPeriod,
                new DateOnly(2025, 1, 15),
                setup.Account,
                setup.Fund,
                setup.OtherFund,
                100.00m);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(AccountingPeriodScenarioSetup setup) =>
        new()
        {
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = 1,
            FromFundName = setup.Fund.Name,
            ToFundName = setup.OtherFund.Name,
            Amount = 100.00m,
        };
}
#pragma warning restore CA1724