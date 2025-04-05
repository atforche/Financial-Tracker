using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Scenarios.Transaction;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Tests adding a Transaction with different Transaction Amount scenarios
/// </summary>
public class AmountTest
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(TransactionAmountScenarios))]
    public void RunTest(BalanceEventAmountScenario scenario, TransactionAccountType? accountType)
    {
        var setup = new TransactionAmountScenarioSetup(scenario);
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
    private static Transaction AddTransaction(TransactionAmountScenarioSetup setup, TransactionAccountType? accountType)
    {
        GetAccountsForTestCase(setup, accountType, out Account? debitAccount, out Account? creditAccount);
        return setup.GetService<IAccountingPeriodService>().AddTransaction(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            debitAccount,
            creditAccount,
            [
                new FundAmount()
                {
                    Fund = setup.Fund,
                    Amount = setup.Amount,
                }
            ]);
    }

    /// <summary>
    /// Gets the Accounts that should be used for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountType">Account Type for this test case</param>
    /// <param name="debitAccount">Debit Account to use for this test case</param>
    /// <param name="creditAccount">Credit Account to use for this test case</param>
    private static void GetAccountsForTestCase(
        TransactionAmountScenarioSetup setup,
        TransactionAccountType? accountType,
        out Account? debitAccount,
        out Account? creditAccount)
    {
        if (accountType == TransactionAccountType.Credit)
        {
            debitAccount = null;
            creditAccount = setup.DebtAccount;
            return;
        }
        debitAccount = setup.StandardAccount;
        creditAccount = null;
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="accountType">Account Type for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(TransactionAmountScenarioSetup setup, TransactionAccountType? accountType)
    {
        GetAccountsForTestCase(setup, accountType, out Account? debitAccount, out Account? creditAccount);
        List<TransactionBalanceEventState> balanceEvents = [];
        if (debitAccount != null)
        {
            balanceEvents.Add(new TransactionBalanceEventState
            {
                AccountName = debitAccount.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                TransactionEventType = TransactionBalanceEventType.Added,
                TransactionAccountType = TransactionAccountType.Debit,
            });
        }
        if (creditAccount != null)
        {
            balanceEvents.Add(new TransactionBalanceEventState
            {
                AccountName = creditAccount.Name,
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