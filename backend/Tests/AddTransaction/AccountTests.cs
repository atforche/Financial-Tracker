using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios.Transaction;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different Transaction Account scenarios
/// </summary>
public class AccountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddTransactionAccountScenarios))]
    public void RunTest(AccountType? debitAccountType, AccountType? creditAccountType, SameAccountTypeBehavior sameAccountTypeBehavior)
    {
        var setup = new TransactionAccountScenarioSetup(debitAccountType, creditAccountType, sameAccountTypeBehavior);
        if (ShouldThrowException(setup))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup), GetExpectedState(setup));
        if (setup.DebitAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<IAccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.DebitAccount, new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31))),
                [GetExpectedAccountBalance(setup, setup.DebitAccount)]);
        }
        if (setup.CreditAccount != null)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<IAccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.CreditAccount, new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31))),
                [GetExpectedAccountBalance(setup, setup.CreditAccount)]);
        }
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(TransactionAccountScenarioSetup setup) => setup.DebitAccount == setup.CreditAccount;

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
    private static List<TransactionBalanceEventState> GetExpectedBalanceEvents(TransactionAccountScenarioSetup setup)
    {
        List<TransactionBalanceEventState> expectedBalanceEvents = [];
        if (setup.DebitAccount != null)
        {
            expectedBalanceEvents.Add(new TransactionBalanceEventState
            {
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
    private static AccountBalanceByEventState GetExpectedAccountBalance(TransactionAccountScenarioSetup setup, Account account) =>
        new()
        {
            AccountName = account.Name,
            AccountingPeriodYear = setup.AccountingPeriod.Year,
            AccountingPeriodMonth = setup.AccountingPeriod.Month,
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
    private static int DetermineBalanceChangeFactor(TransactionAccountScenarioSetup setup, Account account)
    {
        if (account == setup.DebitAccount)
        {
            return account.Type == AccountType.Debt ? 1 : -1;
        }
        return account.Type == AccountType.Debt ? -1 : 1;
    }
}