using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Tests.Mocks;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByEventTests;

/// <summary>
/// Test class that tests getting an Account Balance by Event with different <see cref="GetAccountBalanceAccountingPeriodOverlapScenarios"/>
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
        var setup = new GetAccountBalanceAccountingPeriodOverlapScenarioSetup(accountingPeriodType, eventDate);
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, accountingPeriodType, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.PastAccountingPeriod);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, accountingPeriodType, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.CurrentAccountingPeriod);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, accountingPeriodType, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.FutureAccountingPeriod);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, accountingPeriodType, eventDate));
    }

    /// <summary>
    /// Gets the Account Balance by Events for the provided Accounting Period
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Events for the provided Accounting Period</returns>
    private static IEnumerable<AccountBalanceByEvent> GetAccountBalance(GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup) =>
        setup.GetService<AccountBalanceService>().GetAccountBalancesByEvent(setup.Account.Id,
            new DateRange(new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 20)));

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static List<AccountBalanceByEventState> GetExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriodType accountingPeriodType,
        DateOnly eventDate) =>
        GetCurrentPeriodExpectedState(setup, eventDate)
            .Concat(GetPastPeriodExpectedState(setup, accountingPeriodType, eventDate))
            .Concat(GetFuturePeriodExpectedState(setup, accountingPeriodType, eventDate))
            .ToList();

    /// <summary>
    /// Gets the expected states for the current Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected states for the current Accounting Period for this test case</returns>
    private static List<AccountBalanceByEventState> GetCurrentPeriodExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        DateOnly eventDate)
    {
        List<AccountBalanceByEventState> results = [];
        results.Add(new AccountBalanceByEventState
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = 1,
            AccountId = setup.Account.Id,
            FundBalances =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = eventDate < new DateOnly(2025, 1, 15) ? 750.00m : 1250.00m
                }
            ],
            PendingFundBalanceChanges = []
        });
        return results;
    }

    /// <summary>
    /// Gets the expected states for the past Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected states for the past Accounting Period for this test case</returns>
    private static List<AccountBalanceByEventState> GetPastPeriodExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriodType accountingPeriodType,
        DateOnly eventDate)
    {
        List<AccountBalanceByEventState> results = [];
        if (accountingPeriodType == AccountingPeriodType.Past && eventDate == new DateOnly(2025, 1, 15))
        {
            results.Add(new AccountBalanceByEventState
            {
                AccountingPeriodId = setup.PastAccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 2,
                AccountId = setup.Account.Id,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 750.00m
                    }
                ],
                PendingFundBalanceChanges = []
            });
        }
        return results;
    }

    /// <summary>
    /// Gets the expected states for the future Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected states for the future Accounting Period for this test case</returns>
    private static List<AccountBalanceByEventState> GetFuturePeriodExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriodType accountingPeriodType,
        DateOnly eventDate)
    {
        List<AccountBalanceByEventState> results = [];
        if (accountingPeriodType == AccountingPeriodType.Future && eventDate == new DateOnly(2025, 1, 15))
        {
            results.Add(new AccountBalanceByEventState
            {
                AccountingPeriodId = setup.FutureAccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 2,
                AccountId = setup.Account.Id,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 750.00m
                    }
                ],
                PendingFundBalanceChanges = []
            });
        }
        return results;
    }
}