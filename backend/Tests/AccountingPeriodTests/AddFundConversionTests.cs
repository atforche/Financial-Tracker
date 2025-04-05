using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests adding a Fund Conversion to an Accounting Period
/// </summary>
public class AddFundConversionTests
{
    /// <summary>
    /// Tests that a Fund Conversion can be added successfully
    /// </summary>
    [Fact]
    public void SimpleTest()
    {
        var setup = new DefaultSetup();
        FundConversion fundConversion = setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            100.00m);
        new FundConversionValidator().Validate(fundConversion,
            new FundConversionState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                FromFundName = setup.Fund.Name,
                ToFundName = setup.OtherFund.Name,
                Amount = 100.00m,
            });
    }

    /// <summary>
    /// Tests adding a Fund Conversion with different Accounting Period scenarios
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
            Assert.Throws<InvalidOperationException>(() => setup.GetService<IAccountingPeriodService>()
                .AddFundConversion(setup.CurrentAccountingPeriod,
                    new DateOnly(2025, 1, 15),
                    setup.Account,
                    setup.Fund,
                    setup.OtherFund,
                    100.00m));
            return;
        }
        // Otherwise, ensure the Fund Conversion can be added normally
        FundConversion fundConversion = setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.CurrentAccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            100.00m);
        new FundConversionValidator().Validate(fundConversion,
            new FundConversionState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                FromFundName = setup.Fund.Name,
                ToFundName = setup.OtherFund.Name,
                Amount = 100.00m,
            });
    }

    /// <summary>
    /// Test adding a Fund Conversion with different event dates
    /// </summary>
    [Theory]
    [MemberData(nameof(EventDateSetup.GetCollection), MemberType = typeof(EventDateSetup))]
    public void EventDateTests(DateOnly eventDate)
    {
        var setup = new EventDateSetup();
        if (setup.CalculateMonthDifference(eventDate) > 1)
        {
            // Ensure that an error is thrown if the Fund Conversion is added more than one month outside of the Accounting Period
            Assert.Throws<InvalidOperationException>(() => setup.GetService<IAccountingPeriodService>()
                .AddFundConversion(setup.CurrentAccountingPeriod,
                    eventDate,
                    setup.Account,
                    setup.Fund,
                    setup.OtherFund,
                    100.00m));
            return;
        }
        // Otherwise, ensure the Fund Conversion can be added normally
        FundConversion fundConversion = setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.CurrentAccountingPeriod,
            eventDate,
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            100.00m);
        new FundConversionValidator().Validate(fundConversion,
            new FundConversionState
            {
                AccountName = setup.Account.Name,
                EventDate = eventDate,
                EventSequence = 1,
                FromFundName = setup.Fund.Name,
                ToFundName = setup.OtherFund.Name,
                Amount = 100.00m,
            });
    }

    /// <summary>
    /// Test adding a Fund Conversion with the different Account scenarios
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountScenarios))]
    public void AccountTests(AccountType accountType)
    {
        var setup = new AccountScenarioSetup(accountType);

        // Add a Fund Conversion debiting and crediting the fund
        FundConversion debitFundConversion = setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.AccountingPeriod,
            new DateOnly(2025, 1, 10),
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            100.00m);
        FundConversion creditFundConversion = setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.AccountingPeriod,
            new DateOnly(2025, 1, 20),
            setup.Account,
            setup.OtherFund,
            setup.Fund,
            200.00m);

        // Verify that the Fund Conversion was added as expected
        new FundConversionValidator().Validate(debitFundConversion,
            new FundConversionState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 10),
                EventSequence = 1,
                FromFundName = setup.Fund.Name,
                ToFundName = setup.OtherFund.Name,
                Amount = 100.00m,
            });
        new FundConversionValidator().Validate(creditFundConversion,
            new FundConversionState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 20),
                EventSequence = 1,
                FromFundName = setup.OtherFund.Name,
                ToFundName = setup.Fund.Name,
                Amount = 200.00m,
            });

        // Verify that the Account balance was affected as expected
        new AccountBalanceByEventValidator().Validate(
            setup.GetService<IAccountBalanceService>().GetAccountBalancesByEvent(setup.Account, new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31))),
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
                            Amount = 1400.00m,
                        },
                        new FundAmountState
                        {
                            FundName = setup.OtherFund.Name,
                            Amount = 1600.00m,
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
                            Amount = 1600.00m,
                        },
                        new FundAmountState
                        {
                            FundName = setup.OtherFund.Name,
                            Amount = 1400.00m,
                        }
                    ],
                    PendingFundBalanceChanges = [],
                }
            ]);
    }

    /// <summary>
    /// Test adding a Fund Conversion with different combinations of Funds
    /// </summary>
    [Fact]
    public void FundTests()
    {
        var setup = new DefaultSetup();
        // Test that having the same from Fund and to Fund will fail
        Assert.Throws<InvalidOperationException>(() =>
            setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.AccountingPeriod,
                new DateOnly(2024, 11, 15),
                setup.Account,
                setup.Fund,
                setup.OtherFund,
                100.00m));
    }

    /// <summary>
    /// Tests adding a Fund Conversion with different Balance Event Amount scenarios
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventAmountScenarios))]
    public void AmountTests(BalanceEventAmountScenario scenario)
    {
        List<BalanceEventAmountScenario> expectedErrors =
        [
            BalanceEventAmountScenario.Zero,
            BalanceEventAmountScenario.Negative,
            BalanceEventAmountScenario.ForcesFundBalanceNegative,
            BalanceEventAmountScenario.ForcesAccountBalanceToZero,
            BalanceEventAmountScenario.ForcesAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative
        ];
        var setup = new BalanceEventAmountScenarioSetup(scenario);
        if (expectedErrors.Contains(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.AccountingPeriod,
                new DateOnly(2025, 1, 10),
                setup.Account,
                setup.Fund,
                setup.OtherFund,
                setup.Amount));
            return;
        }
        FundConversion fundConversion = setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.AccountingPeriod,
            new DateOnly(2025, 1, 10),
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            setup.Amount);
        new FundConversionValidator().Validate(fundConversion,
            new FundConversionState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 10),
                EventSequence = 1,
                FromFundName = setup.Fund.Name,
                ToFundName = setup.OtherFund.Name,
                Amount = setup.Amount,
            });
    }
}