using Domain.Accounts;
using Tests.Old.Scenarios;
using Tests.Old.Setups;
using Tests.Old.Validators;

namespace Tests.Old.GetAccountBalanceByDateTests;

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
        var setup = new GetAccountBalanceDateRangeScenarioSetup(scenario);
        new AccountBalanceByDateValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Gets the Account Balance by Dates for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Dates for this test case</returns>
    private static IEnumerable<AccountBalanceByDate> GetAccountBalance(GetAccountBalanceDateRangeScenarioSetup setup) =>
        setup.GetService<AccountBalanceService>().GetAccountBalancesByDateRange(setup.Account.Id, setup.DateRange);

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
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m
                    }
                  ],
            PendingFundBalanceChanges = [],
        });
}