using Domain.Funds;
using Domain.Transactions;
using Tests.Old.Setups;
using Tests.Old.Validators;

namespace Tests.Old.AddTransaction;

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
            [
                new FundAmount()
                {
                    FundId = setup.Fund.Id,
                    Amount = 25.00m,
                }
            ],
            null,
            null);
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
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedDebit
                        ]
                    }
                ]
            });
    }
}