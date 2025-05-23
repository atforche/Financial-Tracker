using Domain;
using Domain.Actions;
using Domain.Funds;
using Domain.Services;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByDateTests;

/// <summary>
/// Test class that tests getting the Account Balance by Date with a default scenario
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
        new AccountBalanceByDateValidator().Validate(
            setup.GetService<AccountBalanceService>().GetAccountBalancesByDate(setup.Account,
                new DateRange(new DateOnly(2025, 1, 14), new DateOnly(2025, 1, 16))),
            [
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2025, 1, 14),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = setup.Fund.Id,
                            Amount = 1500.00m
                        },
                        new FundAmountState
                        {
                            FundId = setup.OtherFund.Id,
                            Amount = 1500.00m
                        }
                    ],
                    PendingFundBalanceChanges = []
                },
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2025, 1, 15),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = setup.Fund.Id,
                            Amount = 1500.00m
                        },
                        new FundAmountState
                        {
                            FundId = setup.OtherFund.Id,
                            Amount = 1500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = setup.Fund.Id,
                            Amount = -250.00m
                        }
                    ]
                },
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2025, 1, 16),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = setup.Fund.Id,
                            Amount = 1500.00m
                        },
                        new FundAmountState
                        {
                            FundId = setup.OtherFund.Id,
                            Amount = 1500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = setup.Fund.Id,
                            Amount = -250.00m
                        }
                    ]
                }
            ]);
    }
}