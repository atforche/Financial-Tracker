using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with a default scenario
/// </summary>
public class SimpleTest
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
                new FundAmount()
                {
                    Fund = setup.Fund,
                    Amount = 25.00m,
                }
            ]);
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                TransactionDate = new DateOnly(2025, 1, 15),
                AccountingEntries =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 25.00m,
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
                    }
                ]
            });
    }
}