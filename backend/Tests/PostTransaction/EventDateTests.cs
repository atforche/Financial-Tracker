using Domain.Actions;
using Domain.Funds;
using Domain.Transactions;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.PostTransaction;

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
        using var setup = new AddBalanceEventDateScenarioSetup(eventDate);
        Transaction transaction = AddTransaction(setup);
        setup.GetService<ITransactionRepository>().Add(transaction);
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
    private static Transaction AddTransaction(AddBalanceEventDateScenarioSetup setup) =>
        setup.GetService<AddTransactionAction>().Run(setup.CurrentAccountingPeriod,
            new DateOnly(2025, 1, 1),
            setup.Account,
            null,
            [
                new FundAmount()
                {
                    FundId = setup.Fund.Id,
                    Amount = 25.00m,
                }
            ]);

    /// <summary>
    /// Posts the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="transaction">Transaction to be posted</param>
    private static void PostTransaction(AddBalanceEventDateScenarioSetup setup, Transaction transaction) =>
        setup.GetService<PostTransactionAction>().Run(transaction, TransactionAccountType.Debit, setup.EventDate);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AddBalanceEventDateScenarioSetup setup) =>
        new()
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            Date = new DateOnly(2025, 1, 1),
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
                    AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 1),
                    EventSequence = 1,
                    AccountName = setup.Account.Name,
                    EventType = TransactionBalanceEventType.Added,
                    AccountType = TransactionAccountType.Debit,
                },
                new TransactionBalanceEventState
                {
                    AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
                    EventDate = setup.EventDate,
                    EventSequence = setup.EventDate == new DateOnly(2025, 1, 1) ? 2 : 1,
                    AccountName = setup.Account.Name,
                    EventType = TransactionBalanceEventType.Posted,
                    AccountType = TransactionAccountType.Debit,
                },
            ]
        };
}