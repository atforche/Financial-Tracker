using Domain;
using Domain.Accounts;
using Domain.FundConversions;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;
using Tests.Old.Setups;
using Tests.Old.Validators;

namespace Tests.Old.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with different <see cref="AccountScenarios"/> 
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
                .GetAccountBalancesByEvent(setup.Account.Id, new DateRange(new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 10))),
            [GetExpectedAccountBalance(setup, false), GetExpectedAccountBalance(setup, true)]);
    }

    /// <summary>
    /// Adds the Fund Conversion for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="reverse">True to convert funds from Other Fund to Fund, false to convert funds from Fund to Other Fund</param>
    /// <returns>The Fund Conversion that was added for this test case</returns>
    private static FundConversion AddFundConversion(AccountScenarioSetup setup, bool reverse)
    {
        FundConversion fundConversion = setup.GetService<FundConversionFactory>().Create(new CreateFundConversionRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            AccountId = setup.Account.Id,
            FromFundId = reverse ? setup.OtherFund.Id : setup.Fund.Id,
            ToFundId = reverse ? setup.Fund.Id : setup.OtherFund.Id,
            Amount = 100.00m
        });
        setup.GetService<IFundConversionRepository>().Add(fundConversion);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        return fundConversion;
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="reverse">True to convert funds from Other Fund to Fund, false to convert funds from Fund to Other Fund</param>
    /// <returns>The expected state for this test case</returns>
    private static FundConversionState GetExpectedState(AccountScenarioSetup setup, bool reverse) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = reverse ? 2 : 1,
            AccountId = setup.Account.Id,
            FromFundId = reverse ? setup.OtherFund.Id : setup.Fund.Id,
            ToFundId = reverse ? setup.Fund.Id : setup.OtherFund.Id,
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
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 10),
            EventSequence = reverse ? 2 : 1,
            AccountId = setup.Account.Id,
            FundBalances =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = reverse ? 1500.00m : 1400.00m,
                },
                new FundAmountState
                {
                    FundId = setup.OtherFund.Id,
                    Amount = reverse ? 1500.00m : 1600.00m,
                }
            ],
            PendingFundBalanceChanges = []
        };
}