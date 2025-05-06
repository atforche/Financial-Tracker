using Domain.Actions;
using Domain.Services;
using Domain.ValueObjects;
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
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, accountingPeriodType, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.CurrentAccountingPeriod);
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, accountingPeriodType, eventDate));

        setup.GetService<CloseAccountingPeriodAction>().Run(setup.FutureAccountingPeriod);
        new AccountBalanceByEventValidator().Validate(GetAccountBalance(setup), GetExpectedState(setup, accountingPeriodType, eventDate));
    }

    /// <summary>
    /// Gets the Account Balance by Events for the provided Accounting Period
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Account Balance by Events for the provided Accounting Period</returns>
    private static IEnumerable<AccountBalanceByEvent> GetAccountBalance(GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup) =>
        setup.GetService<AccountBalanceService>().GetAccountBalancesByEvent(setup.Account,
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
        GetPastPeriodExpectedState(setup, accountingPeriodType, eventDate)
            .Concat(GetCurrentPeriodExpectedState(setup, accountingPeriodType, eventDate))
            .Concat(GetFuturePeriodExpectedState(setup, accountingPeriodType, eventDate))
            .ToList();

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
                AccountingPeriodKey = setup.PastAccountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1500.00m
                    }
                ],
                PendingFundBalanceChanges =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = -500.00m
                    }
                ]
            });
            results.Add(new AccountBalanceByEventState
            {
                AccountingPeriodKey = setup.PastAccountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 2,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1000.00m
                    }
                ],
                PendingFundBalanceChanges = []
            });
        }
        return results;
    }

    /// <summary>
    /// Gets the expected states for the current Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected states for the current Accounting Period for this test case</returns>
    private static List<AccountBalanceByEventState> GetCurrentPeriodExpectedState(
        GetAccountBalanceAccountingPeriodOverlapScenarioSetup setup,
        AccountingPeriodType accountingPeriodType,
        DateOnly eventDate)
    {
        List<AccountBalanceByEventState> results = [];
        results.Add(new AccountBalanceByEventState
        {
            AccountingPeriodKey = setup.CurrentAccountingPeriod.Key,
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = 1,
            FundBalances =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = eventDate < new DateOnly(2025, 1, 15) || (eventDate == new DateOnly(2025, 1, 15) && accountingPeriodType == AccountingPeriodType.Past)
                        ? 1000.00m
                        : 1500.00m
                }
            ],
            PendingFundBalanceChanges =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = -250.00m
                }
            ]
        });
        results.Add(new AccountBalanceByEventState
        {
            AccountingPeriodKey = setup.CurrentAccountingPeriod.Key,
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = 2,
            FundBalances =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = eventDate < new DateOnly(2025, 1, 15) || (eventDate == new DateOnly(2025, 1, 15) && accountingPeriodType == AccountingPeriodType.Past)
                        ? 750.00m
                        : 1250.00m
                }
            ],
            PendingFundBalanceChanges = []
        });
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
                AccountingPeriodKey = setup.FutureAccountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1250.00m
                    }
                ],
                PendingFundBalanceChanges =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = -500.00m
                    }
                ]
            });
            results.Add(new AccountBalanceByEventState
            {
                AccountingPeriodKey = setup.FutureAccountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 2,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 750.00m
                    }
                ],
                PendingFundBalanceChanges = []
            });
        }
        return results;
    }
}