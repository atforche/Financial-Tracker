using Domain.AccountingPeriods;
using Domain.Actions;
using Domain.Funds;
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
        using var setup = new DefaultScenarioSetup();
        Transaction transaction = setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            null,
            [
                new FundAmount
                {
                    FundId = setup.Fund.Id,
                    Amount = 250.00m,
                }
            ]);
        PostTransaction(setup, transaction);
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                Date = new DateOnly(2025, 1, 15),
                AccountingEntries =
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
                        AccountingPeriodKey = setup.AccountingPeriod.Key,
                        AccountName = setup.Account.Name,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        EventType = TransactionBalanceEventType.Added,
                        AccountType = TransactionAccountType.Debit,
                    },
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodKey = setup.AccountingPeriod.Key,
                        AccountName = setup.Account.Name,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 2,
                        EventType = TransactionBalanceEventType.Posted,
                        AccountType = TransactionAccountType.Debit,
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
        setup.GetService<PostTransactionAction>().Run(transaction, TransactionAccountType.Debit, new DateOnly(2025, 1, 15));
}