using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;
using Tests.Old.Setups;

namespace Tests.Old.AddTransaction.Setups;

/// <summary>
/// Setup class for a <see cref="AddBalanceEventMultipleBalanceEventScenarios"/> for adding a Transaction
/// </summary>
internal sealed class MultipleBalanceEventScenarioSetup : ScenarioSetup
{
    private readonly AccountingPeriod _futureAccountingPeriod;

    /// <summary>
    /// Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Fund for the Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for the Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Account for the Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Debt Account for the Setup
    /// </summary>
    public Account DebtAccount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public MultipleBalanceEventScenarioSetup(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        Fund = GetService<FundFactory>().Create("Test", "");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundFactory>().Create("Test2", "");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        _futureAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(_futureAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    FundId = OtherFund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        GetService<TestUnitOfWork>().SaveChanges();

        DebtAccount = GetService<AccountFactory>().Create("TestDebt", AccountType.Debt, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    FundId = OtherFund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(DebtAccount);
        GetService<TestUnitOfWork>().SaveChanges();

        DoScenarioSpecificSetup(scenario);
    }

    /// <summary>
    /// Performs scenario specific setup for the provided scenario
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    private void DoScenarioSpecificSetup(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.MultipleEventsSameDay)
        {
            List<FundAmount> fundAmounts =
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 500.00m
                }
            ];
            Transaction transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 15),
                Account.Id,
                fundAmounts,
                DebtAccount.Id,
                fundAmounts);
            GetService<ITransactionRepository>().Add(transaction);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesFundBalanceNegative)
        {
            List<FundAmount> fundAmounts =
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1250.00m
                }
            ];
            Transaction transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 10),
                Account.Id,
                fundAmounts,
                DebtAccount.Id,
                fundAmounts);
            GetService<ITransactionRepository>().Add(transaction);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceToZero)
        {
            List<FundAmount> fundAmounts =
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 2500.00m
                }
            ];
            Transaction transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 10),
                Account.Id,
                fundAmounts,
                DebtAccount.Id,
                fundAmounts);
            GetService<ITransactionRepository>().Add(transaction);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceNegative)
        {
            List<FundAmount> fundAmounts =
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 2750.00m
                }
            ];
            Transaction transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 10),
                Account.Id,
                fundAmounts,
                DebtAccount.Id,
                fundAmounts);
            GetService<ITransactionRepository>().Add(transaction);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesFutureEventToMakeAccountBalanceNegative)
        {
            List<FundAmount> fundAmounts =
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 2750.00m
                }
            ];
            Transaction transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 20),
                Account.Id,
                fundAmounts,
                DebtAccount.Id,
                fundAmounts);
            GetService<ITransactionRepository>().Add(transaction);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative)
        {
            List<FundAmount> fundAmounts =
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 2750.00m
                }
            ];
            Transaction transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 10),
                Account.Id,
                fundAmounts,
                DebtAccount.Id,
                fundAmounts);
            GetService<ITransactionRepository>().Add(transaction);
            GetService<TestUnitOfWork>().SaveChanges();

            fundAmounts =
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 2750.00m
                }
            ];
            transaction = GetService<TransactionFactory>().Create(_futureAccountingPeriod.Id,
                new DateOnly(2025, 1, 10),
                DebtAccount.Id,
                fundAmounts,
                Account.Id,
                fundAmounts);
            GetService<ITransactionRepository>().Add(transaction);
            GetService<TestUnitOfWork>().SaveChanges();
        }
    }
}