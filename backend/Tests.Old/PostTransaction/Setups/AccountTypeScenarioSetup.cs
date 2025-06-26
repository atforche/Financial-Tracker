using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Old.Mocks;
using Tests.Old.PostTransaction.Scenarios;
using Tests.Old.Setups;

namespace Tests.Old.PostTransaction.Setups;

/// <summary>
/// Setup class for <see cref="AccountTypeScenarios"/> for posting a Transaction
/// </summary>
internal sealed class AccountTypeScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Accounting Period for this Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Fund for this Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Debit Account for this Setup
    /// </summary>
    public Account DebitAccount { get; }

    /// <summary>
    /// Credit Account for this Setup
    /// </summary>
    public Account CreditAccount { get; }

    /// <summary>
    /// Transaction for this Setup
    /// </summary>
    public Transaction Transaction { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public AccountTypeScenarioSetup(AccountTypeScenario scenario)
    {
        Fund = GetService<FundFactory>().Create("Test");
        GetService<IFundRepository>().Add(Fund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        DebitAccount = GetService<AccountFactory>().Create("Debit", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(DebitAccount);
        GetService<TestUnitOfWork>().SaveChanges();

        CreditAccount = GetService<AccountFactory>().Create("Credit", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(CreditAccount);
        GetService<TestUnitOfWork>().SaveChanges();

        List<FundAmount> fundAmounts =
        [
            new FundAmount
            {
                FundId = Fund.Id,
                Amount = 500.00m
            }
        ];
        Transaction = scenario is AccountTypeScenario.Debit or AccountTypeScenario.MissingCredit
            ? GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 15),
                DebitAccount.Id,
                fundAmounts,
                null,
                null)
            : GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 15),
                null,
                null,
                CreditAccount.Id,
                fundAmounts);
        GetService<ITransactionRepository>().Add(Transaction);
        GetService<TestUnitOfWork>().SaveChanges();
    }
}