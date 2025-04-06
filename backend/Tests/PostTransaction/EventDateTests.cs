using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.PostTransaction;

/// <summary>
/// Test class that tests posting a Transaction with different Balance Event Date scenarios
/// </summary>
public class EventDateTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventDateScenarios))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new BalanceEventDateScenarioSetup(eventDate);
        Transaction transaction = AddTransaction(setup);
        if (ShouldThrowException(setup))
        {
            Assert.Throws<InvalidOperationException>(() => PostTransaction(setup, transaction));
            return;
        }
        PostTransaction(setup, transaction);
        new TransactionValidator().Validate(transaction, GetExpectedState(setup));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(BalanceEventDateScenarioSetup setup)
    {
        if (setup.EventDate < new DateOnly(2025, 1, 1))
        {
            return true;
        }
        return setup.CalculateMonthDifference() > 1;
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(BalanceEventDateScenarioSetup setup) =>
        setup.GetService<IAccountingPeriodService>().AddTransaction(setup.CurrentAccountingPeriod,
            new DateOnly(2025, 1, 1),
            setup.Account,
            null,
            [
                new FundAmount()
                {
                    Fund = setup.Fund,
                    Amount = 25.00m,
                }
            ]);

    /// <summary>
    /// Posts the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="transaction">Transaction to be posted</param>
    private static void PostTransaction(BalanceEventDateScenarioSetup setup, Transaction transaction) =>
        setup.GetService<IAccountingPeriodService>().PostTransaction(transaction, setup.Account, setup.EventDate);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(BalanceEventDateScenarioSetup setup) =>
        new()
        {
            TransactionDate = new DateOnly(2025, 1, 1),
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
                    EventDate = new DateOnly(2025, 1, 1),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                },
                new TransactionBalanceEventState
                {
                    AccountName = setup.Account.Name,
                    EventDate = setup.EventDate,
                    EventSequence = setup.EventDate == new DateOnly(2025, 1, 1) ? 2 : 1,
                    TransactionEventType = TransactionBalanceEventType.Posted,
                    TransactionAccountType = TransactionAccountType.Debit,
                },
            ]
        };
}