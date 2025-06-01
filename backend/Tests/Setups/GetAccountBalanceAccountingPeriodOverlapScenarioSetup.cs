using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Domain.Transactions;
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
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);

        PastAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(PastAccountingPeriod);

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, PastAccountingPeriod.Id, PastAccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        CurrentAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(CurrentAccountingPeriod);

        FutureAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(FutureAccountingPeriod);

        Transaction transaction = GetService<TransactionFactory>().Create(CurrentAccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            Account.Id,
            null,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 250.00m
                }
            ]);
        GetService<ITransactionRepository>().Add(transaction);
        GetService<PostTransactionAction>().Run(transaction, TransactionAccountType.Debit, transaction.Date);

        Transaction otherPeriodTransaction = GetService<TransactionFactory>().Create(
            accountingPeriodType == AccountingPeriodType.Past ? PastAccountingPeriod.Id : FutureAccountingPeriod.Id,
            eventDate,
            Account.Id,
            null,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 500.00m
                }
            ]
        );
        GetService<ITransactionRepository>().Add(otherPeriodTransaction);
        GetService<PostTransactionAction>().Run(otherPeriodTransaction, TransactionAccountType.Debit, otherPeriodTransaction.Date);
    }
}