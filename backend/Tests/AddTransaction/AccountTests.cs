using Domain;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.AddTransaction.Scenarios;
using Tests.AddTransaction.Setups;
using Tests.Mocks;
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
                    .GetAccountBalancesByEvent(setup.DebitAccount.Id, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                [GetExpectedAccountBalance(setup, setup.DebitAccount)]);
        }
        if (setup.CreditAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<AccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.CreditAccount.Id, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                [GetExpectedAccountBalance(setup, setup.CreditAccount)]);
        }
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(AccountScenarioSetup setup)
    {
        Transaction transaction = setup.GetService<TransactionFactory>().Create(setup.AccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            setup.DebitAccount?.Id,
            setup.CreditAccount?.Id,
            [
                new FundAmount()
                {
                    FundId = setup.Fund.Id,
                    Amount = 25.00m,
                },
                new FundAmount
                {
                    FundId = setup.OtherFund.Id,
                    Amount = 50.00m
                }
            ]);
        setup.GetService<ITransactionRepository>().Add(transaction);
        setup.GetService<TestUnitOfWork>().SaveChanges();
        return transaction;
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AccountScenarioSetup setup) =>
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
                },
                new FundAmountState
                {
                    FundId = setup.OtherFund.Id,
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
                EventSequence = setup.DebitAccount != null ? 2 : 1,
                AccountId = setup.CreditAccount.Id,
                EventType = TransactionBalanceEventType.Added,
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
    private static AccountBalanceByEventState GetExpectedAccountBalance(AccountScenarioSetup setup, Account account) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = setup.DebitAccount != account && setup.DebitAccount != null ? 2 : 1,
            AccountId = account.Id,
            FundBalances =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = 1500.00m,
                },
                new FundAmountState
                {
                    FundId = setup.OtherFund.Id,
                    Amount = 1500.00m,
                }
            ],
            PendingFundBalanceChanges =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = DetermineBalanceChangeFactor(setup, account) * 25.00m,
                },
                new FundAmountState
                {
                    FundId = setup.OtherFund.Id,
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