using Domain;
using Domain.AccountingPeriods;
using Domain.Actions;
using Domain.Funds;
using Domain.Services;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByEventTests;

/// <summary>
/// Test class that tests getting the Account Balance by Event with a default scenario
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
        Transaction transaction = setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            null,
            [
                new FundAmount
                {
                    Fund = setup.Fund,
                    Amount = 250.00m
                }
            ]);
        transaction.Post(TransactionAccountType.Debit, new DateOnly(2025, 1, 16));
        new AccountBalanceByEventValidator().Validate(
            setup.GetService<AccountBalanceService>().GetAccountBalancesByEvent(setup.Account,
                new DateRange(new DateOnly(2025, 1, 14), new DateOnly(2025, 1, 16))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodKey = setup.AccountingPeriod.Key,
                    AccountName = setup.Account.Name,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = 1500.00m
                        },
                        new FundAmountState
                        {
                            FundName = setup.OtherFund.Name,
                            Amount = 1500.00m
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
                },
                new AccountBalanceByEventState
                {
                    AccountingPeriodKey = setup.AccountingPeriod.Key,
                    AccountName = setup.Account.Name,
                    EventDate = new DateOnly(2025, 1, 16),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = 1250.00m
                        },
                        new FundAmountState
                        {
                            FundName = setup.OtherFund.Name,
                            Amount = 1500.00m
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);
    }
}