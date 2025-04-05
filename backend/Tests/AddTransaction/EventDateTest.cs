using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different event dates
/// </summary>
public class EventDateTest
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [MemberData(nameof(EventDateSetup.GetCollection), MemberType = typeof(EventDateSetup))]
    public void RunTest(DateOnly eventDate)
    {
        var setup = new EventDateSetup();
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
    /// <param name="setup">Event Date Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(EventDateSetup setup, DateOnly eventDate) =>
        setup.CalculateMonthDifference(eventDate) > 1;

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Event Date Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The transaction that was added fo this test case</returns>
    private static Transaction AddTransaction(EventDateSetup setup, DateOnly eventDate) =>
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
    /// <param name="setup">Event Date Setup for this test case</param>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(EventDateSetup setup, DateOnly eventDate) =>
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