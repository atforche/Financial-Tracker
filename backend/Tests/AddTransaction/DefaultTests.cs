using Domain.Funds;
using Domain.Transactions;
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
        var setup = new DefaultScenarioSetup();
        Transaction transaction = setup.GetService<TransactionFactory>().Create(setup.AccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            setup.Account.Id,
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
                AccountingPeriodId = setup.AccountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                FundAmounts =
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
                        AccountingPeriodId = setup.AccountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        AccountId = setup.Account.Id,
                        EventType = TransactionBalanceEventType.Added,
                        AccountType = TransactionAccountType.Debit,
                    }
                ]
            });
    }
}