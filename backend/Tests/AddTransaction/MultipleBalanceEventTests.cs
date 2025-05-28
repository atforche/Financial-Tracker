using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Domain.Transactions;
using Tests.AddTransaction.Setups;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different <see cref="AddBalanceEventMultipleBalanceEventScenarios"/>
/// </summary>
public class MultipleBalanceEventTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventMultipleBalanceEventScenarios))]
    public void RunTest(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        using var setup = new MultipleBalanceEventScenarioSetup(scenario);
        if (!IsValid(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup, AccountType.Standard));
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup, AccountType.Debt));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup, AccountType.Standard), GetExpectedState(setup, scenario, AccountType.Standard));
        new TransactionValidator().Validate(AddTransaction(setup, AccountType.Debt), GetExpectedState(setup, scenario, AccountType.Debt));
    }

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    private static bool IsValid(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        List<AddBalanceEventMultipleBalanceEventScenario> invalidScenarios =
        [
            AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceNegative,
            AddBalanceEventMultipleBalanceEventScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative,
        ];
        return !invalidScenarios.Contains(scenario);
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountType">Account Type for this Transaction</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(MultipleBalanceEventScenarioSetup setup, AccountType accountType)
    {
        Transaction transaction = setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            accountType == AccountType.Standard ? setup.Account : null,
            accountType == AccountType.Debt ? setup.DebtAccount : null,
            [
                new FundAmount
                {
                    FundId = setup.Fund.Id,
                    Amount = 500.00m,
                }
            ]);
        setup.GetService<ITransactionRepository>().Add(transaction);
        return transaction;
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="scenario">Scenario for this test case</param>
    /// <param name="accountType">Account Type for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(
        MultipleBalanceEventScenarioSetup setup,
        AddBalanceEventMultipleBalanceEventScenario scenario,
        AccountType accountType)
    {
        int expectedEventSequence = 1;
        if (accountType == AccountType.Debt)
        {
            expectedEventSequence++;
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.MultipleEventsSameDay)
        {
            expectedEventSequence += 2;
        }

        return new TransactionState
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            Date = new DateOnly(2025, 1, 15),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = 500.00m,
                }
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountingPeriodId = setup.AccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = expectedEventSequence,
                    AccountName = accountType == AccountType.Standard ? setup.Account.Name : setup.DebtAccount.Name,
                    EventType = TransactionBalanceEventType.Added,
                    AccountType = accountType == AccountType.Standard ? TransactionAccountType.Debit : TransactionAccountType.Credit
                }
            ]
        };
    }
}