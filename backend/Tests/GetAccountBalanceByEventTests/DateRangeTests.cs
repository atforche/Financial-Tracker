using Domain.Accounts;
using Domain.Services;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByEventTests;

/// <summary>
/// Test class that tests getting an Account Balance by Event with different <see cref="GetAccountBalanceDateRangeScenarios"/>
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
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Gets the Account Balance by Events for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Events for this test case</returns>
    private static IEnumerable<AccountBalanceByEvent> GetAccountBalance(GetAccountBalanceDateRangeScenarioSetup setup) =>
        setup.GetService<AccountBalanceService>().GetAccountBalancesByEvent(setup.Account, setup.DateRange);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static IEnumerable<AccountBalanceByEventState> GetExpectedState(GetAccountBalanceDateRangeScenarioSetup setup)
    {
        if (!setup.DateRange.IsInRange(setup.Account.AccountAddedBalanceEvent.EventDate))
        {
            return [];
        }
        return
        [
            new AccountBalanceByEventState
            {
                AccountingPeriodKey = setup.Account.AccountAddedBalanceEvent.AccountingPeriodKey,
                AccountName = setup.Account.Name,
                EventDate = setup.Account.AccountAddedBalanceEvent.EventDate,
                EventSequence = 1,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m,
                    }
                ],
                PendingFundBalanceChanges = []
            }
        ];
    }
}