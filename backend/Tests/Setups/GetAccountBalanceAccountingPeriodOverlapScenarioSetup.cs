using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Mocks;
using Tests.Scenarios;

namespace Tests.Setups;

/// <summary>
/// Setup class for a <see cref="GetAccountBalanceAccountingPeriodOverlapScenarios"/> for getting an Account Balance
/// </summary>
internal sealed class GetAccountBalanceAccountingPeriodOverlapScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Fund for the Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Account for the Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Past Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod PastAccountingPeriod { get; }

    /// <summary>
    /// Current Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod CurrentAccountingPeriod { get; }

    /// <summary>
    /// Future Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod FutureAccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodType">Accounting Period Type for this test case</param>
    /// <param name="eventDate">Balance Event Date for this test case</param>
    public GetAccountBalanceAccountingPeriodOverlapScenarioSetup(AccountingPeriodType accountingPeriodType, DateOnly eventDate)
    {
        Fund = GetService<FundFactory>().Create("Test");
        GetService<IFundRepository>().Add(Fund);
        GetService<TestUnitOfWork>().SaveChanges();

        PastAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(PastAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, PastAccountingPeriod.Id, PastAccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        GetService<TestUnitOfWork>().SaveChanges();

        CurrentAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(CurrentAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        FutureAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(FutureAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Transaction transaction = GetService<TransactionFactory>().Create(CurrentAccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            Account.Id,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 250.00m
                }
            ],
            null,
            null);
        GetService<ITransactionRepository>().Add(transaction);
        GetService<TestUnitOfWork>().SaveChanges();

        GetService<PostTransactionAction>().Run(transaction, Account.Id, transaction.Date);
        GetService<TestUnitOfWork>().SaveChanges();

        Transaction otherPeriodTransaction = GetService<TransactionFactory>().Create(
            accountingPeriodType == AccountingPeriodType.Past ? PastAccountingPeriod.Id : FutureAccountingPeriod.Id,
            eventDate,
            Account.Id,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 500.00m
                }
            ],
            null,
            null
        );
        GetService<ITransactionRepository>().Add(otherPeriodTransaction);
        GetService<TestUnitOfWork>().SaveChanges();

        GetService<PostTransactionAction>().Run(otherPeriodTransaction, Account.Id, otherPeriodTransaction.Date);
        GetService<TestUnitOfWork>().SaveChanges();
    }
}