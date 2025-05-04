using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Scenarios.Transaction;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different Transaction Amount scenarios
/// </summary>
public class AmountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(TransactionAmountScenarios))]
    public void RunTest(BalanceEventAmountScenario scenario, TransactionAccountType? accountType)
    {
        var setup = new TransactionAmountScenarioSetup(scenario, accountType);
        if (ShouldThrowException(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup, accountType));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup, accountType), GetExpectedState(setup, accountType));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(BalanceEventAmountScenario scenario)
    {
        List<BalanceEventAmountScenario> invalidScenarios =
        [
            BalanceEventAmountScenario.Zero,
            BalanceEventAmountScenario.Negative,
            BalanceEventAmountScenario.ForcesAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            BalanceEventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative
        ];
        return invalidScenarios.Contains(scenario);
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountType">Account Type for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(TransactionAmountScenarioSetup setup, TransactionAccountType? accountType) =>
        setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            accountType != TransactionAccountType.Credit ? setup.Account : null,
            accountType == TransactionAccountType.Credit ? setup.Account : null,
            [
                new FundAmount()
                {
                    Fund = setup.Fund,
                    Amount = setup.Amount,
                }
            ]);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountType">Account Type for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(TransactionAmountScenarioSetup setup, TransactionAccountType? accountType)
    {
        List<TransactionBalanceEventState> balanceEvents = [];
        if (accountType != TransactionAccountType.Credit)
        {
            balanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                TransactionEventType = TransactionBalanceEventType.Added,
                TransactionAccountType = TransactionAccountType.Debit,
            });
        }
        else
        {
            balanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                TransactionEventType = TransactionBalanceEventType.Added,
                TransactionAccountType = TransactionAccountType.Credit,
            });
        }
        return new()
        {
            TransactionDate = new DateOnly(2025, 1, 15),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = setup.Amount,
                }
            ],
            TransactionBalanceEvents = balanceEvents
        };
    }
}