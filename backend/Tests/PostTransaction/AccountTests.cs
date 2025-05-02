using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios.Transaction;
using Tests.Validators;

namespace Tests.PostTransaction;

/// <summary>
/// Test class that tests posting a Transaction with different Transaction Account scenarios
/// </summary>
public class AccountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(TransactionAccountScenarios))]
    public void RunTest(AccountType? debitAccountType, AccountType? creditAccountType, SameAccountTypeBehavior sameAccountTypeBehavior)
    {
        var setup = new TransactionAccountScenarioSetup(debitAccountType, creditAccountType, sameAccountTypeBehavior);
        Transaction transaction = AddTransaction(setup);

        // Verify that posting a Transaction with a random account results in an error
        if (debitAccountType == null)
        {
            Assert.Throws<InvalidOperationException>(() => transaction.Post(TransactionAccountType.Debit, new DateOnly(2025, 1, 25)));
        }
        if (creditAccountType == null)
        {
            Assert.Throws<InvalidOperationException>(() => transaction.Post(TransactionAccountType.Credit, new DateOnly(2025, 1, 25)));
        }

        // Verify posting the transaction normally
        PostTransaction(setup, transaction);
        new TransactionValidator().Validate(transaction, GetExpectedState(setup));
        if (setup.DebitAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<IAccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.DebitAccount, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                GetExpectedAccountBalance(setup, setup.DebitAccount));
        }
        if (setup.CreditAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<IAccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.CreditAccount, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                GetExpectedAccountBalance(setup, setup.CreditAccount));
        }
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(TransactionAccountScenarioSetup setup) =>
        setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.DebitAccount,
            setup.CreditAccount,
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
    private static void PostTransaction(TransactionAccountScenarioSetup setup, Transaction transaction)
    {
        if (setup.DebitAccount != null)
        {
            transaction.Post(TransactionAccountType.Debit, new DateOnly(2025, 1, 15));
        }
        if (setup.CreditAccount != null)
        {
            transaction.Post(TransactionAccountType.Credit, new DateOnly(2025, 1, 15));
        }
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(TransactionAccountScenarioSetup setup) =>
        new()
        {
            TransactionDate = new DateOnly(2025, 1, 15),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 25.00m,
                }
            ],
            TransactionBalanceEvents = GetExpectedBalanceEvents(setup),
        };

    /// <summary>
    /// Gets the expected Transaction Balance Events for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected Transaction Balance Events for this test case</returns>
    private static List<TransactionBalanceEventState> GetExpectedBalanceEvents(TransactionAccountScenarioSetup setup)
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
                EventSequence = expectedBalanceEvents.Count + 1,
                TransactionEventType = TransactionBalanceEventType.Added,
                TransactionAccountType = TransactionAccountType.Credit
            });
        }
        if (setup.DebitAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                AccountName = setup.DebitAccount.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = expectedBalanceEvents.Count + 1,
                TransactionEventType = TransactionBalanceEventType.Posted,
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
                EventSequence = expectedBalanceEvents.Count + 1,
                TransactionEventType = TransactionBalanceEventType.Posted,
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
    private static List<AccountBalanceByEventState> GetExpectedAccountBalance(TransactionAccountScenarioSetup setup, Account account) =>
        [
            new()
            {
                AccountName = account.Name,
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = DetermineBalanceEventSequence(setup, account, TransactionBalanceEventType.Added),
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
                ],
            },
            new()
            {
                AccountName = account.Name,
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = DetermineBalanceEventSequence(setup, account, TransactionBalanceEventType.Posted),
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = setup.Fund.Name,
                        Amount = 1500.00m + (DetermineBalanceChangeFactor(setup, account) * 25.00m),
                    },
                    new FundAmountState
                    {
                        FundName = setup.OtherFund.Name,
                        Amount = 1500.00m,
                    }
                ],
                PendingFundBalanceChanges = [],
            },
        ];

    /// <summary>
    /// Determines the Balance Event Sequence that should be associated with the provided event
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="account">Account for this Balance Event</param>
    /// <param name="eventType">Event Type for this Balance Event</param>
    /// <returns>The Balance Event Sequence that should be associated with the provided event</returns>
    private static int DetermineBalanceEventSequence(
        TransactionAccountScenarioSetup setup,
        Account account,
        TransactionBalanceEventType eventType)
    {
        if (eventType == TransactionBalanceEventType.Added)
        {
            if (setup.DebitAccount == account)
            {
                return 1;
            }
            if (setup.DebitAccount != null)
            {
                return 2;
            }
            return 1;
        }
        if (setup.DebitAccount == account)
        {
            if (setup.CreditAccount == null)
            {
                return 2;
            }
            return 3;
        }
        if (setup.DebitAccount == null)
        {
            return 2;
        }
        return 4;
    }

    /// <summary>
    /// Determines the expected balance change factor for the provided test case and Account
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="account">Account to get the balance change factor for</param>
    /// <returns>The expected balance change factor for the provided test case and Account</returns>
    private static int DetermineBalanceChangeFactor(TransactionAccountScenarioSetup setup, Account account)
    {
        if (account == setup.DebitAccount)
        {
            return account.Type == AccountType.Debt ? 1 : -1;
        }
        return account.Type == AccountType.Debt ? -1 : 1;
    }
}