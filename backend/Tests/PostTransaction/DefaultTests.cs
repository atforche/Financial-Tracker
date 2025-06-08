using Domain.Funds;
using Domain.Transactions;
using Tests.Mocks;
using Tests.Setups;
using Tests.Validators;

namespace Tests.PostTransaction;

/// <summary>
/// Test class that tests posting a Transaction with a default scenario
/// </summary>
public class DefaultTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Fact]
    public void RunTest()
    {
        var setup = new DefaultScenarioSetup();
        Transaction transaction = setup.GetService<TransactionFactory>().Create(setup.AccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            setup.Account.Id,
            [
                new FundAmount
                {
                    FundId = setup.Fund.Id,
                    Amount = 250.00m,
                }
            ],
            null,
            null);
        setup.GetService<ITransactionRepository>().Add(transaction);
        setup.GetService<TestUnitOfWork>().SaveChanges();

        PostTransaction(setup, transaction);
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = setup.Account.Id,
                DebitFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 250.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = setup.AccountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedDebit,
                            TransactionBalanceEventPartType.PostedDebit,
                        ]
                    },
                ]
            });

        // Verify that double posting a transaction results in an error
        Assert.Throws<InvalidOperationException>(() => PostTransaction(setup, transaction));
    }

    /// <summary>
    /// Posts the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="transaction">Transaction to be posted</param>
    private static void PostTransaction(DefaultScenarioSetup setup, Transaction transaction)
    {
        setup.GetService<PostTransactionAction>().Run(transaction, setup.Account.Id, new DateOnly(2025, 1, 15));
        setup.GetService<TestUnitOfWork>().SaveChanges();
    }
}