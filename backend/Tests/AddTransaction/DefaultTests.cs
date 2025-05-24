using Domain.AccountingPeriods;
using Domain.Actions;
using Domain.Funds;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with a default scenario
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
                new FundAmount()
                {
                    FundId = setup.Fund.Id,
                    Amount = 25.00m,
                }
            ]);
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                Date = new DateOnly(2025, 1, 15),
                AccountingEntries =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 25.00m,
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
                    }
                ]
            });
    }
}