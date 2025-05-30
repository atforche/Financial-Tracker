using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Domain.Services;
using Tests.AddTransaction.Scenarios;
using Tests.AddTransaction.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different <see cref="AccountScenarios"/>
/// </summary>
public class AccountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountScenarios))]
    public void RunTest(AccountType? debitAccountType, AccountType? creditAccountType, SameAccountTypeBehavior sameAccountTypeBehavior)
    {
        var setup = new AccountScenarioSetup(debitAccountType, creditAccountType, sameAccountTypeBehavior);
        if (!AccountScenarios.IsValid(debitAccountType, creditAccountType, sameAccountTypeBehavior))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup), GetExpectedState(setup));
        if (setup.DebitAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<AccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.DebitAccount, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                [GetExpectedAccountBalance(setup, setup.DebitAccount)]);
        }
        if (setup.CreditAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<AccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.CreditAccount, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                [GetExpectedAccountBalance(setup, setup.CreditAccount)]);
        }
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(AccountScenarioSetup setup) =>
        setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.DebitAccount,
            setup.CreditAccount,
            [
                new FundAmount()
                {
                    Fund = setup.Fund,
                    Amount = 25.00m,
                },
                new FundAmount
                {
                    Fund = setup.OtherFund,
                    Amount = 50.00m
                }
            ]);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AccountScenarioSetup setup) =>
        new()
        {
            TransactionDate = new DateOnly(2025, 1, 15),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 25.00m,
                },
                new FundAmountState
                {
                    FundName = setup.OtherFund.Name,
                    Amount = 50.00m,
                }
            ],
            TransactionBalanceEvents = GetExpectedBalanceEvents(setup),
        };

    /// <summary>
    /// Gets the expected Transaction Balance Events for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected Transaction Balance Events for this test case</returns>
    private static List<TransactionBalanceEventState> GetExpectedBalanceEvents(AccountScenarioSetup setup)
    {
        List<TransactionBalanceEventState> expectedBalanceEvents = [];
        if (setup.DebitAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                AccountName = setup.DebitAccount.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                TransactionEventType = TransactionBalanceEventType.Added,
                TransactionAccountType = TransactionAccountType.Debit
            });
        }
        if (setup.CreditAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                AccountName = setup.CreditAccount.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = setup.DebitAccount != null ? 2 : 1,
                TransactionEventType = TransactionBalanceEventType.Added,
                TransactionAccountType = TransactionAccountType.Credit
            });
        }
        return expectedBalanceEvents;
    }

    /// <summary>
    /// Gets the expected Account Balance for this test case and Account
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="account">Account to get the expected balance for</param>
    /// <returns>The expected Account Balance for this test case and Account</returns>
    private static AccountBalanceByEventState GetExpectedAccountBalance(AccountScenarioSetup setup, Account account) =>
        new()
        {
            AccountName = account.Name,
            AccountingPeriodKey = setup.AccountingPeriod.Key,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = setup.DebitAccount != account && setup.DebitAccount != null ? 2 : 1,
            FundBalances =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 1500.00m,
                },
                new FundAmountState
                {
                    FundName = setup.OtherFund.Name,
                    Amount = 1500.00m,
                }
            ],
            PendingFundBalanceChanges =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = DetermineBalanceChangeFactor(setup, account) * 25.00m,
                },
                new FundAmountState
                {
                    FundName = setup.OtherFund.Name,
                    Amount = DetermineBalanceChangeFactor(setup, account) * 50.00m,
                }
            ],
        };

    /// <summary>
    /// Determines the expected balance change factor for the provided test case and Account
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="account">Account to get the balance change factor for</param>
    /// <returns>The expected balance change factor for the provided test case and Account</returns>
    private static int DetermineBalanceChangeFactor(AccountScenarioSetup setup, Account account)
    {
        if (account == setup.DebitAccount)
        {
            return account.Type == AccountType.Debt ? 1 : -1;
        }
        return account.Type == AccountType.Debt ? -1 : 1;
    }
}