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
        new TransactionValidator().Validate(setup.Transaction, GetExpectedState(setup, scenario));
        if (scenario is AccountTypeScenario.Debit or AccountTypeScenario.MissingCredit)
        {
            new AccountBalanceByEventValidator().Validate(
                setup.GetService<AccountBalanceService>()
                    .GetAccountBalancesByEvent(setup.DebitAccount.Id, new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
                GetExpectedAccountBalance(setup, setup.DebitAccount));
        }
        else
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
            setup.GetService<PostTransactionAction>().Run(setup.Transaction, setup.DebitAccount!.Id, new DateOnly(2025, 1, 15));
        }
        else
        {
            setup.GetService<PostTransactionAction>().Run(setup.Transaction, setup.CreditAccount!.Id, new DateOnly(2025, 1, 15));
        }
        setup.GetService<TestUnitOfWork>().SaveChanges();
    }

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AccountTypeScenarioSetup setup, AccountTypeScenario scenario)
    {
        List<FundAmountState> fundAmounts =
        [
            new FundAmountState
            {
                FundId = setup.Fund.Id,
                Amount = 500.00m,
            }
        ];
        if (scenario is AccountTypeScenario.Debit or AccountTypeScenario.MissingCredit)
        {
            return new()
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = setup.DebitAccount.Id,
                DebitFundAmounts = fundAmounts,
                TransactionBalanceEvents = GetExpectedBalanceEvents(setup, scenario),
            };
        }
        return new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            Date = new DateOnly(2025, 1, 15),
            CreditAccountId = setup.CreditAccount.Id,
            CreditFundAmounts = fundAmounts,
            TransactionBalanceEvents = GetExpectedBalanceEvents(setup, scenario),
        };
    }

    /// <summary>
    /// Gets the expected Transaction Balance Events for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>The expected Transaction Balance Events for this test case</returns>
    private static List<TransactionBalanceEventState> GetExpectedBalanceEvents(AccountTypeScenarioSetup setup, AccountTypeScenario scenario)
    {
        List<TransactionBalanceEventPartType> expectedBalanceEventParts = [];

        if (scenario is AccountTypeScenario.Debit or AccountTypeScenario.MissingCredit)
        {
            expectedBalanceEventParts.Add(TransactionBalanceEventPartType.AddedDebit);
            expectedBalanceEventParts.Add(TransactionBalanceEventPartType.PostedDebit);
        }
        else
        {
            expectedBalanceEventParts.Add(TransactionBalanceEventPartType.AddedCredit);
            expectedBalanceEventParts.Add(TransactionBalanceEventPartType.PostedCredit);
        }
        return
        [
            new TransactionBalanceEventState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                Parts = expectedBalanceEventParts
            }
        ];
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
                EventSequence = 1,
                AccountId = account.Id,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m + (DetermineBalanceChangeFactor(setup, account) * 500.00m),
                    }
                ],
                PendingFundBalanceChanges = []
            },
        ];

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