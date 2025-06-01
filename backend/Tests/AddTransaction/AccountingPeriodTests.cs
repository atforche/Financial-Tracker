using Domain.Funds;
using Domain.Transactions;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different <see cref="AddBalanceEventAccountingPeriodScenarios"/>
/// </summary>
public class AccountingPeriodTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventAccountingPeriodScenarios))]
    public void RunTest(AddBalanceEventAccountingPeriodScenario scenario)
    {
        using var setup = new AddBalanceEventAccountingPeriodScenarioSetup(scenario);
        if (!AddBalanceEventAccountingPeriodScenarios.IsValid(scenario))
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
    private static Transaction AddTransaction(AddBalanceEventAccountingPeriodScenarioSetup setup) =>
        setup.GetService<TransactionFactory>().Create(setup.AccountingPeriod.Id,
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

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AddBalanceEventAccountingPeriodScenarioSetup setup) =>
        new()
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
        };
}