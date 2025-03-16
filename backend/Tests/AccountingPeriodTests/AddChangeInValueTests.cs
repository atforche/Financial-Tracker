using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests adding a Change In Value to an Accounting Period
/// </summary>
public class AddChangeInValueTests : UnitTestBase
{
    /// <summary>
    /// Tests that a Change In Value can be added successfully
    /// </summary>
    [Fact]
    public void SimpleTest()
    {
        var setup = new DefaultSetup();
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
    /// Tests adding a Change In Value with different Accounting Period scenarios
    /// </summary>
    [Theory]
    [MemberData(nameof(AccountingPeriodSetup.GetCollection), MemberType = typeof(AccountingPeriodSetup))]
    public void AccountingPeriodTests(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        var setup = new AccountingPeriodSetup(pastPeriodStatus, currentPeriodStatus, futurePeriodStatus);
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
    /// Test adding a Change In Value with different event dates
    /// </summary>
    [Theory]
    [MemberData(nameof(EventDateSetup.GetCollection), MemberType = typeof(EventDateSetup))]
    public void EventDateTests(DateOnly eventDate)
    {
        var setup = new EventDateSetup();
        int monthDifference = (Math.Abs(setup.CurrentAccountingPeriod.Year - eventDate.Year) * 12) + Math.Abs(setup.CurrentAccountingPeriod.Month - eventDate.Month);
        if (monthDifference > 1)
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
    /// Test adding a Change In Value to different types of Accounts
    /// </summary>
    [Theory]
    [MemberData(nameof(AccountSetup.GetCollection), MemberType = typeof(AccountSetup))]
    public void AccountTypeTests(AccountType accountType)
    {
        var setup = new AccountSetup(accountType);

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
                    EventDate = positiveChangeInValue.EventDate,
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = 2600.00m,
                        }
                    ],
                    PendingFundBalanceChanges = [],
                },
                new AccountBalanceByEventState
                {
                    AccountName = setup.Account.Name,
                    AccountingPeriodYear = setup.AccountingPeriod.Year,
                    AccountingPeriodMonth = setup.AccountingPeriod.Month,
                    EventDate = negativeChangeInValue.EventDate,
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = 2500.00m,
                        }
                    ],
                    PendingFundBalanceChanges = [],
                },
            ]);
    }

    /// <summary>
    /// Test adding a Change In Value with different amounts
    /// </summary>
    [Theory]
    [MemberData(nameof(EventAmountSetup.GetCollection), MemberType = typeof(EventAmountSetup))]
    public void FundAmountTests(EventAmountScenario scenario)
    {
        List<EventAmountScenario> expectedErrors =
        [
            EventAmountScenario.Zero,
            EventAmountScenario.ForcesAccountBalanceNegative,
            EventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            EventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative
        ];
        var setup = new EventAmountSetup(scenario);
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
                EventDate = changeInValue.EventDate,
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = setup.Amount,
                }
            });
    }
}