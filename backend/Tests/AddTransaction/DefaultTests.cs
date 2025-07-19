using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Builders;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a basic Transaction
/// </summary>
public class DefaultTests : TestClass
{
    /// <summary>
    /// Runs the default test for adding a Transaction
    /// </summary>
    [Fact]
    public void RunTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account account = GetService<AccountBuilder>().Build();

        Transaction transaction = GetService<TransactionBuilder>()
            .WithDebitAccount(account.Id)
            .WithDebitFundAmounts(
            [
                new FundAmount
                {
                    FundId = fund.Id,
                    Amount = 500.00m
                }
            ])
            .Build();
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = accountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = account.Id,
                DebitFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = accountingPeriod.Id,
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