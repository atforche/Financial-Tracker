using Domain.Accounts;
using Tests.Old.Scenarios;
using Tests.Old.Setups;
using Tests.Old.Validators;

namespace Tests.Old.GetAccountBalanceByEventTests;

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
        var setup = new GetAccountBalanceDateRangeScenarioSetup(scenario);
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Gets the Account Balance by Events for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Events for this test case</returns>
    private static IEnumerable<AccountBalanceByEvent> GetAccountBalance(GetAccountBalanceDateRangeScenarioSetup setup) =>
        setup.GetService<AccountBalanceService>().GetAccountBalancesByEvent(setup.Account.Id, setup.DateRange);

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
                AccountingPeriodId = setup.Account.AccountAddedBalanceEvent.AccountingPeriodId,
                EventDate = setup.Account.AccountAddedBalanceEvent.EventDate,
                EventSequence = 1,
                AccountId = setup.Account.Id,
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