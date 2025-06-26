using Domain.Funds;
using Domain.Transactions;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;
using Tests.Old.Setups;
using Tests.Old.Validators;

namespace Tests.Old.PostTransaction;

/// <summary>
/// Test class that tests posting a Transaction with different <see cref="AddBalanceEventDateScenarios"/>
/// </summary>
public class EventDateTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventDateScenarios))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new AddBalanceEventDateScenarioSetup(eventDate);
        Transaction transaction = AddTransaction(setup);
        if (!AddBalanceEventDateScenarios.IsValid(eventDate) || eventDate < new DateOnly(2025, 1, 1))
        {
            Assert.Throws<InvalidOperationException>(() => PostTransaction(setup, transaction));
            return;
        }
        PostTransaction(setup, transaction);
        new TransactionValidator().Validate(transaction, GetExpectedState(setup));
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(AddBalanceEventDateScenarioSetup setup)
    {
        List<FundAmount> fundAmounts =
        [
            new FundAmount()
            {
                FundId = setup.Fund.Id,
                Amount = 25.00m,
            }
        ];
        Transaction transaction = setup.GetService<TransactionFactory>().Create(setup.CurrentAccountingPeriod.Id,
            new DateOnly(2025, 1, 1),
            setup.Account.Id,
            fundAmounts,
            setup.OtherAccount.Id,
            fundAmounts);
        setup.GetService<ITransactionRepository>().Add(transaction);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        return transaction;
    }

    /// <summary>
    /// Posts the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="transaction">Transaction to be posted</param>
    private static void PostTransaction(AddBalanceEventDateScenarioSetup setup, Transaction transaction)
    {
        setup.GetService<PostTransactionAction>().Run(transaction, setup.Account.Id, setup.EventDate);
        setup.GetService<TestUnitOfWork>().SaveChanges();

        setup.GetService<PostTransactionAction>().Run(transaction, setup.OtherAccount.Id, setup.EventDate);
        setup.GetService<TestUnitOfWork>().SaveChanges();
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AddBalanceEventDateScenarioSetup setup)
    {
        List<TransactionBalanceEventState> expectedBalanceEvents = [];
        if (setup.EventDate == new DateOnly(2025, 1, 1))
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 1),
                EventSequence = 1,
                Parts =
                [
                    TransactionBalanceEventPartType.AddedDebit,
                    TransactionBalanceEventPartType.AddedCredit,
                    TransactionBalanceEventPartType.PostedDebit,
                    TransactionBalanceEventPartType.PostedCredit,
                ]
            });
        }
        else
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 1),
                EventSequence = 1,
                Parts =
                [
                    TransactionBalanceEventPartType.AddedDebit,
                    TransactionBalanceEventPartType.AddedCredit,
                ]
            });
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
                EventDate = setup.EventDate,
                EventSequence = 1,
                Parts =
                [
                    TransactionBalanceEventPartType.PostedDebit,
                    TransactionBalanceEventPartType.PostedCredit,
                ]
            });
        }
        List<FundAmountState> fundAmounts =
        [
            new FundAmountState
            {
                FundId = setup.Fund.Id,
                Amount = 25.00m,
            }
        ];
        return new()
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            Date = new DateOnly(2025, 1, 1),
            DebitAccountId = setup.Account.Id,
            DebitFundAmounts = fundAmounts,
            CreditAccountId = setup.OtherAccount.Id,
            CreditFundAmounts = fundAmounts,
            TransactionBalanceEvents = expectedBalanceEvents
        };
    }
}