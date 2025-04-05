using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests adding a Change In Value to an Accounting Period
/// </summary>
public class AddChangeInValueTests
{
    /// <summary>
    /// Tests adding a Change In Value with a default scenario
    /// </summary>
    [Fact]
    public void SimpleTest()
    {
        var setup = new DefaultScenarioSetup();
        ChangeInValue changeInValue = setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = -100.00m,
            });
        new ChangeInValueValidator().Validate(changeInValue,
            new ChangeInValueState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = -100.00m,
                }
            });
    }

    /// <summary>
    /// Tests adding a Change In Value with the different Accounting Period scenarios
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountingPeriodScenarios))]
    public void AccountingPeriodTests(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        var setup = new AccountingPeriodScenarioSetup(pastPeriodStatus, currentPeriodStatus, futurePeriodStatus);
        if (!setup.CurrentAccountingPeriod.IsOpen)
        {
            // Ensure that an error is thrown for a closed Accounting Period
            Assert.Throws<InvalidOperationException>(() => setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.CurrentAccountingPeriod,
                new DateOnly(2025, 1, 15),
                setup.Account,
                new FundAmount
                {
                    Fund = setup.Fund,
                    Amount = -100.00m,
                }));
            return;
        }
        // Otherwise, ensure the Change In Value can be added normally
        ChangeInValue changeInValue = setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.CurrentAccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = -100.00m,
            });
        new ChangeInValueValidator().Validate(changeInValue,
            new ChangeInValueState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = -100.00m,
                }
            });
    }

    /// <summary>
    /// Test adding a Change In Value with different Balance Event Date scenarios
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventDateScenarios))]
    public void BalanceEventDateTests(DateOnly eventDate)
    {
        var setup = new BalanceEventDateScenarioSetup();
        if (setup.CalculateMonthDifference(eventDate) > 1)
        {
            // Ensure that an error is thrown if the Change In Value is added more than one month outside of the Accounting Period
            Assert.Throws<InvalidOperationException>(() => setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.CurrentAccountingPeriod,
                eventDate,
                setup.Account,
                new FundAmount
                {
                    Fund = setup.Fund,
                    Amount = -100.00m,
                }));
            return;
        }
        // Otherwise, ensure the Change In Value can be added normally
        ChangeInValue changeInValue = setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.CurrentAccountingPeriod,
            eventDate,
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = -100.00m,
            });
        new ChangeInValueValidator().Validate(changeInValue,
            new ChangeInValueState
            {
                AccountName = setup.Account.Name,
                EventDate = eventDate,
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = -100.00m,
                }
            });
    }

    /// <summary>
    /// Test adding a Change In Value with the different Account scenarios
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountScenarios))]
    public void AccountTests(AccountType accountType)
    {
        var setup = new AccountScenarioSetup(accountType);

        // Add a positive and negative change in balance
        ChangeInValue positiveChangeInValue = setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.AccountingPeriod,
            new DateOnly(2025, 1, 10),
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = 100.00m,
            });
        ChangeInValue negativeChangeInValue = setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.AccountingPeriod,
            new DateOnly(2025, 1, 20),
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = -100.00m,
            });

        // Verify that the Change In Values were added as expected
        new ChangeInValueValidator().Validate(positiveChangeInValue,
            new ChangeInValueState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 10),
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 100.00m,
                }
            });
        new ChangeInValueValidator().Validate(negativeChangeInValue,
            new ChangeInValueState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 20),
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = -100.00m,
                }
            });
        // Verify that the Account balance was affected as expected
        new AccountBalanceByEventValidator().Validate(
            setup.GetService<IAccountBalanceService>()
                .GetAccountBalancesByEvent(setup.Account, new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = setup.Account.Name,
                    AccountingPeriodYear = setup.AccountingPeriod.Year,
                    AccountingPeriodMonth = setup.AccountingPeriod.Month,
                    EventDate = new DateOnly(2025, 1, 10),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = 1600.00m,
                        },
                        new FundAmountState
                        {
                            FundName = setup.OtherFund.Name,
                            Amount = 1500.00m,
                        }
                    ],
                    PendingFundBalanceChanges = [],
                },
                new AccountBalanceByEventState
                {
                    AccountName = setup.Account.Name,
                    AccountingPeriodYear = setup.AccountingPeriod.Year,
                    AccountingPeriodMonth = setup.AccountingPeriod.Month,
                    EventDate = new DateOnly(2025, 1, 20),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = 1500.00m,
                        },
                        new FundAmountState
                        {
                            FundName = setup.OtherFund.Name,
                            Amount = 1500.00m,
                        }
                    ],
                    PendingFundBalanceChanges = [],
                },
            ]);
    }

    /// <summary>
    /// Test adding a Change In Value with different Balance Event Amount scenarios
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventAmountScenarios))]
    public void AmountTests(BalanceEventAmountScenario scenario)
    {
        List<BalanceEventAmountScenario> expectedErrors =
        [
            BalanceEventAmountScenario.Zero,
            BalanceEventAmountScenario.ForcesAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative
        ];
        var setup = new BalanceEventAmountScenarioSetup(scenario);
        if (expectedErrors.Contains(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.AccountingPeriod,
                new DateOnly(2025, 1, 10),
                setup.Account,
                new FundAmount
                {
                    Fund = setup.Fund,
                    Amount = setup.Amount,
                }));
            return;
        }
        ChangeInValue changeInValue = setup.GetService<IAccountingPeriodService>().AddChangeInValue(setup.AccountingPeriod,
            new DateOnly(2025, 1, 10),
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = setup.Amount,
            });
        new ChangeInValueValidator().Validate(changeInValue,
            new ChangeInValueState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 10),
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = setup.Amount,
                }
            });
    }
}