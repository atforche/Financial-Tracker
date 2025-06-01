using Domain;
using Domain.Funds;
using Domain.Services;
using Domain.Transactions;
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
        Transaction transaction = setup.GetService<TransactionFactory>().Create(setup.AccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            setup.Account.Id,
            null,
            [
                new FundAmount
                {
                    FundId = setup.Fund.Id,
                    Amount = 250.00m
                }
            ]);
        setup.GetService<ITransactionRepository>().Add(transaction);
        new AccountBalanceByDateValidator().Validate(
            setup.GetService<AccountBalanceService>().GetAccountBalancesByDateRange(setup.Account.Id,
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