using Domain.AccountingPeriods;
using Domain.Actions;
using Domain.Funds;
using Domain.Services;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByAccountingPeriodTests;

/// <summary>
/// Test class that tests getting the Account Balance by Accounting Period with a default scenario
/// </summary>
public class DefaultTests
{
    /// <summary>
    /// Runs the test for this test case
    /// </summary>
    [Fact]
    public void RunTest()
    {
        using var setup = new DefaultScenarioSetup();
        setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            null,
            [
                new FundAmount
                {
                    FundId = setup.Fund.Id,
                    Amount = 250.00m
                }
            ]);
        new AccountBalanceByAccountingPeriodValidator().Validate(
            setup.GetService<AccountBalanceService>().GetAccountBalancesByAccountingPeriod(setup.Account, setup.AccountingPeriod),
            new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodKey = new AccountingPeriodKey(2025, 1),
                StartingFundBalances = [],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m,
                    },
                    new FundAmountState
                    {
                        FundId = setup.OtherFund.Id,
                        Amount = 1500.00m,
                    }
                ],
                EndingPendingFundBalanceChanges =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = -250.00m,
                    }
                ]
            });
    }
}