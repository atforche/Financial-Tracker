using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different Balance Event Date scenarios
/// </summary>
public class EventDateTest
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventDateScenarios))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new BalanceEventDateScenarioSetup();
        if (ShouldThrowException(setup, eventDate))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup, eventDate));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup, eventDate), GetExpectedState(setup, eventDate));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(BalanceEventDateScenarioSetup setup, DateOnly eventDate) =>
        setup.CalculateMonthDifference(eventDate) > 1;

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The transaction that was added fo this test case</returns>
    private static Transaction AddTransaction(BalanceEventDateScenarioSetup setup, DateOnly eventDate) =>
        setup.GetService<IAccountingPeriodService>().AddTransaction(setup.CurrentAccountingPeriod,
            eventDate,
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
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(BalanceEventDateScenarioSetup setup, DateOnly eventDate) =>
        new()
        {
            TransactionDate = eventDate,
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
                    EventDate = eventDate,
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        };
}