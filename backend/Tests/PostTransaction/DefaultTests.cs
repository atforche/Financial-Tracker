using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
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
        Transaction transaction = setup.GetService<IAccountingPeriodService>().AddTransaction(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            null,
            [
                new FundAmount
                {
                    Fund = setup.Fund,
                    Amount = 250.00m,
                }
            ]);
        PostTransaction(setup, transaction);
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                TransactionDate = new DateOnly(2025, 1, 15),
                AccountingEntries =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 250.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountName = setup.Account.Name,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        TransactionEventType = TransactionBalanceEventType.Added,
                        TransactionAccountType = TransactionAccountType.Debit,
                    },
                    new TransactionBalanceEventState
                    {
                        AccountName = setup.Account.Name,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 2,
                        TransactionEventType = TransactionBalanceEventType.Posted,
                        TransactionAccountType = TransactionAccountType.Debit,
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
    private static void PostTransaction(DefaultScenarioSetup setup, Transaction transaction) =>
        setup.GetService<IAccountingPeriodService>().PostTransaction(transaction, setup.Account, new DateOnly(2025, 1, 15));
}