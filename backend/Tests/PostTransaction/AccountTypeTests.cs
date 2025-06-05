using Domain;
using Domain.Accounts;
using Domain.Transactions;
using Tests.Mocks;
using Tests.PostTransaction.Scenarios;
using Tests.PostTransaction.Setups;
using Tests.Validators;

namespace Tests.PostTransaction;

/// <summary>
/// Test class that tests posting a Transaction with different <see cref="AccountTypeScenarios"/>
/// </summary>
public class AccountTypeTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AccountTypeScenarios))]
    public void RunTest(AccountTypeScenario scenario)
    {
        var setup = new AccountTypeScenarioSetup(scenario);
        if (!AccountTypeScenarios.IsValid(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => PostTransaction(setup, scenario));
            return;
        }
        PostTransaction(setup, scenario);
        new TransactionValidator().Validate(setup.Transaction, GetExpectedState(setup));
        if (setup.DebitAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<AccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.DebitAccount.Id, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                GetExpectedAccountBalance(setup, setup.DebitAccount));
        }
        if (setup.CreditAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<AccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.CreditAccount.Id, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                GetExpectedAccountBalance(setup, setup.CreditAccount));
        }
    }

    /// <summary>
    /// Posts the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="scenario">Scenario for this test case</param>
    private static void PostTransaction(AccountTypeScenarioSetup setup, AccountTypeScenario scenario)
    {
        if (scenario is AccountTypeScenario.Debit or AccountTypeScenario.MissingDebit)
        {
            setup.GetService<PostTransactionAction>().Run(setup.Transaction, TransactionAccountType.Debit, new DateOnly(2025, 1, 15));
        }
        else
        {
            setup.GetService<PostTransactionAction>().Run(setup.Transaction, TransactionAccountType.Credit, new DateOnly(2025, 1, 15));
        }
        setup.GetService<TestUnitOfWork>().SaveChanges();
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AccountTypeScenarioSetup setup) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            Date = new DateOnly(2025, 1, 15),
            FundAmounts =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = 500.00m,
                }
            ],
            TransactionBalanceEvents = GetExpectedBalanceEvents(setup),
        };

    /// <summary>
    /// Gets the expected Transaction Balance Events for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected Transaction Balance Events for this test case</returns>
    private static List<TransactionBalanceEventState> GetExpectedBalanceEvents(AccountTypeScenarioSetup setup)
    {
        List<TransactionBalanceEventState> expectedBalanceEvents = [];
        if (setup.DebitAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountId = setup.DebitAccount.Id,
                EventType = TransactionBalanceEventType.Added,
                AccountType = TransactionAccountType.Debit
            });
        }
        if (setup.CreditAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = expectedBalanceEvents.Count + 1,
                AccountId = setup.CreditAccount.Id,
                EventType = TransactionBalanceEventType.Added,
                AccountType = TransactionAccountType.Credit
            });
        }
        if (setup.DebitAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = expectedBalanceEvents.Count + 1,
                AccountId = setup.DebitAccount.Id,
                EventType = TransactionBalanceEventType.Posted,
                AccountType = TransactionAccountType.Debit
            });
        }
        if (setup.CreditAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = expectedBalanceEvents.Count + 1,
                AccountId = setup.CreditAccount.Id,
                EventType = TransactionBalanceEventType.Posted,
                AccountType = TransactionAccountType.Credit
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
    private static List<AccountBalanceByEventState> GetExpectedAccountBalance(AccountTypeScenarioSetup setup, Account account) =>
        [
            new()
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = DetermineBalanceEventSequence(setup, account, TransactionBalanceEventType.Added),
                AccountId = account.Id,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m,
                    }
                ],
                PendingFundBalanceChanges =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = DetermineBalanceChangeFactor(setup, account) * 500.00m,
                    },
                ],
            },
            new()
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = DetermineBalanceEventSequence(setup, account, TransactionBalanceEventType.Posted),
                AccountId = account.Id,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m + (DetermineBalanceChangeFactor(setup, account) * 500.00m),
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
        AccountTypeScenarioSetup setup,
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
    private static int DetermineBalanceChangeFactor(AccountTypeScenarioSetup setup, Account account)
    {
        if (account == setup.DebitAccount)
        {
            return account.Type == AccountType.Debt ? 1 : -1;
        }
        return account.Type == AccountType.Debt ? -1 : 1;
    }
}