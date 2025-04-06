using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different Balance Event Amount scenarios
/// </summary>
public class AmountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventAmountScenarios))]
    public void RunTest(BalanceEventAmountScenario scenario)
    {
        var setup = new BalanceEventAmountScenarioSetup(scenario);
        if (ShouldThrowException(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => AddFundConversion(setup));
            return;
        }
        new FundConversionValidator().Validate(AddFundConversion(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(BalanceEventAmountScenario scenario)
    {
        List<BalanceEventAmountScenario> invalidScenarios =
        [
            BalanceEventAmountScenario.Zero,
            BalanceEventAmountScenario.Negative,
            BalanceEventAmountScenario.ForcesFundBalanceNegative,
            BalanceEventAmountScenario.ForcesAccountBalanceToZero,
            BalanceEventAmountScenario.ForcesAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative
        ];
        return invalidScenarios.Contains(scenario);
    }

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(BalanceEventAmountScenarioSetup setup) =>
        setup.GetService<IAccountingPeriodService>()
            .AddFundConversion(setup.AccountingPeriod,
                new DateOnly(2025, 1, 10),
                setup.Account,
                setup.Fund,
                setup.OtherFund,
                setup.Amount);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(BalanceEventAmountScenarioSetup setup) =>
        new()
        {
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = 1,
            FromFundName = setup.Fund.Name,
            ToFundName = setup.OtherFund.Name,
            Amount = setup.Amount,
        };
}