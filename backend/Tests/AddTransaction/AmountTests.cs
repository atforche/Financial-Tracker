using Domain.AccountingPeriods;
using Domain.Actions;
using Domain.Funds;
using Tests.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different <see cref="AddBalanceEventAmountScenarios"/>
/// </summary>
public class AmountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventAmountScenarios))]
    public void RunTest(decimal amount)
    {
        using var setup = new AddBalanceEventAmountScenarioSetup();
        if (!IsValid(amount))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup, amount));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup, amount), GetExpectedState(setup, amount));
    }

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    private static bool IsValid(decimal amount) => amount > 0;

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            null,
            [
                new FundAmount
                {
                    Fund = setup.Fund,
                    Amount = 100.00m,
                },
                new FundAmount
                {
                    Fund = setup.OtherFund,
                    Amount = amount,
                }
            ]);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        new()
        {
            TransactionDate = new DateOnly(2025, 1, 15),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 100.00m,
                },
                new FundAmountState
                {
                    FundName = setup.OtherFund.Name,
                    Amount = amount,
                },
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountingPeriodKey = setup.AccountingPeriod.Key,
                    AccountName = setup.Account.Name,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        };
}