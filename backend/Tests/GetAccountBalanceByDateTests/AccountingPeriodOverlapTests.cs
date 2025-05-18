using Domain;
using Domain.Accounts;
using Domain.Actions;
using Domain.Services;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByDateTests;

/// <summary>
/// Test class that tests getting an Account Balance by Date with different <see cref="GetAccountBalanceAccountingPeriodOverlapScenarios"/>
/// </summary>
public class AccountingPeriodOverlapTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(GetAccountBalanceAccountingPeriodOverlapScenarios))]
    public void RunTest(AccountingPeriodType accountingPeriodType, DateOnly eventDate)
    {
        using var setup = new GetAccountBalanceAccountingPeriodOverlapScenarioSetup(accountingPeriodType, eventDate);
        new AccountBalanceByDateValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.PastAccountingPeriod);
        new AccountBalanceByDateValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.CurrentAccountingPeriod);
        new AccountBalanceByDateValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.FutureAccountingPeriod);
        new AccountBalanceByDateValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, eventDate));
    }

    /// <summary>
    /// Gets the Account Balance by Date for the provided Accounting Period
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Date for the provided Accounting Period</returns>
    private static IEnumerable<AccountBalanceByDate> GetAccountBalance(GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup) =>
        setup.GetService<AccountBalanceService>().GetAccountBalancesByDate(setup.Account,
            new DateRange(new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 20)));

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static IEnumerable<AccountBalanceByDateState> GetExpectedState(GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup, DateOnly eventDate) =>
        new DateRange(new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 20)).GetInclusiveDates()
            .Select(date =>
            {
                decimal amount = 1500.00m;
                if (date >= eventDate)
                {
                    amount -= 500.00m;
                }
                if (date >= new DateOnly(2025, 1, 15))
                {
                    amount -= 250.00m;
                }
                return new AccountBalanceByDateState
                {
                    Date = date,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = amount
                        }
                    ],
                    PendingFundBalanceChanges = []
                };
            });
}