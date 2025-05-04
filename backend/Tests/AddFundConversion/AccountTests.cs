using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different Account scenarios
/// </summary>
public class AccountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountScenarios))]
    public void RunTest(AccountType accountType)
    {
        var setup = new AccountScenarioSetup(accountType);
        new FundConversionValidator().Validate(AddFundConversion(setup, false), GetExpectedState(setup, false));
        new FundConversionValidator().Validate(AddFundConversion(setup, true), GetExpectedState(setup, true));
        new AccountBalanceByEventValidator().Validate(
            setup.GetService<AccountBalanceService>()
                .GetAccountBalancesByEvent(setup.Account, new DateRange(new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 10))),
            [GetExpectedAccountBalance(setup, false), GetExpectedAccountBalance(setup, true)]);
    }

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="reverse">True to convert funds from Other Fund to Fund, false to convert funds from Fund to Other Fund</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(AccountScenarioSetup setup, bool reverse) =>
        setup.GetService<AddFundConversionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 10),
            setup.Account,
            reverse ? setup.OtherFund : setup.Fund,
            reverse ? setup.Fund : setup.OtherFund,
            100.00m);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="reverse">True to convert funds from Other Fund to Fund, false to convert funds from Fund to Other Fund</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(AccountScenarioSetup setup, bool reverse) =>
        new()
        {
            AccountingPeriodKey = setup.AccountingPeriod.Key,
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = reverse ? 2 : 1,
            FromFundName = reverse ? setup.OtherFund.Name : setup.Fund.Name,
            ToFundName = reverse ? setup.Fund.Name : setup.OtherFund.Name,
            Amount = 100.00m,
        };

    /// <summary>
    /// Gets the expected Account Balance for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="reverse">True to convert funds from Other Fund to Fund, false to convert funds from Fund to Other Fund</param>
    /// <returns>The expected Account Balance for this test case</returns>
    private static AccountBalanceByEventState GetExpectedAccountBalance(AccountScenarioSetup setup, bool reverse) =>
        new()
        {
            AccountName = setup.Account.Name,
            AccountingPeriodKey = setup.AccountingPeriod.Key,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = reverse ? 2 : 1,
            FundBalances =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = reverse ? 1500.00m : 1400.00m,
                },
                new FundAmountState
                {
                    FundName = setup.OtherFund.Name,
                    Amount = reverse ? 1500.00m : 1600.00m,
                }
            ],
            PendingFundBalanceChanges = []
        };
}