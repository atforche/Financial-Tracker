using Domain.Funds;
using Domain.Transactions;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different <see cref="AddBalanceEventDateScenarios"/>
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
        if (!AddBalanceEventDateScenarios.IsValid(eventDate))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(AddBalanceEventDateScenarioSetup setup) =>
        setup.GetService<TransactionFactory>().Create(setup.CurrentAccountingPeriod.Id,
            setup.EventDate,
            setup.Account.Id,
            null,
            [
                new FundAmount()
                {
                    FundId = setup.Fund.Id,
                    Amount = 25.00m,
                }
            ]);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AddBalanceEventDateScenarioSetup setup) =>
        new()
        {
            AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
            Date = setup.EventDate,
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
                    AccountingPeriodId = setup.CurrentAccountingPeriod.Id,
                    EventDate = setup.EventDate,
                    EventSequence = 1,
                    AccountId = setup.Account.Id,
                    EventType = TransactionBalanceEventType.Added,
                    AccountType = TransactionAccountType.Debit,
                }
            ]
        };
}