using Domain.Accounts;
using Domain.Services;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByDateTests;

/// <summary>
/// Test class that tests getting an Account Balance by Date with different <see cref="GetAccountBalanceDateRangeScenarios"/>
/// </summary>
public class DateRangeTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(GetAccountBalanceDateRangeScenarios))]
    public void RunTest(GetAccountBalanceDateRangeScenario scenario)
    {
        using var setup = new GetAccountBalanceDateRangeScenarioSetup(scenario);
        new AccountBalanceByDateValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Gets the Account Balance by Dates for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Dates for this test case</returns>
    private static IEnumerable<AccountBalanceByDate> GetAccountBalance(GetAccountBalanceDateRangeScenarioSetup setup) =>
        setup.GetService<AccountBalanceService>().GetAccountBalancesByDate(setup.Account, setup.DateRange);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static IEnumerable<AccountBalanceByDateState> GetExpectedState(GetAccountBalanceDateRangeScenarioSetup setup) =>
        setup.DateRange.GetInclusiveDates().Select(date => new AccountBalanceByDateState
        {
            Date = date,
            FundBalances = date < new DateOnly(2025, 1, 1)
                ? []
                : [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1500.00m
                    }
                  ],
            PendingFundBalanceChanges = [],
        });
}