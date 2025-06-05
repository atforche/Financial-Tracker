using Domain;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Mocks;
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
        var setup = new DefaultScenarioSetup();
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
        setup.GetService<TestUnitOfWork>().SaveChanges();

        setup.GetService<PostTransactionAction>().Run(transaction, TransactionAccountType.Debit, new DateOnly(2025, 1, 16));
        setup.GetService<TestUnitOfWork>().SaveChanges();

        new AccountBalanceByEventValidator().Validate(
            setup.GetService<AccountBalanceService>().GetAccountBalancesByEvent(setup.Account.Id,
                new DateRange(new DateOnly(2025, 1, 14), new DateOnly(2025, 1, 16))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = setup.AccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = setup.Account.Id,
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
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = setup.AccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 16),
                    EventSequence = 1,
                    AccountId = setup.Account.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = setup.Fund.Id,
                            Amount = 1250.00m
                        },
                        new FundAmountState
                        {
                            FundId = setup.OtherFund.Id,
                            Amount = 1500.00m
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);
    }
}