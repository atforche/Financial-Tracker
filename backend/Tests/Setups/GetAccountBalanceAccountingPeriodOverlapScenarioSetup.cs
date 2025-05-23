using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
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

        Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, PastAccountingPeriod, PastAccountingPeriod.PeriodStartDate,
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

        Transaction transaction = GetService<AddTransactionAction>().Run(CurrentAccountingPeriod,
            new DateOnly(2025, 1, 15),
            Account,
            null,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 250.00m
                }
            ]);
        transaction.Post(TransactionAccountType.Debit, transaction.TransactionDate);

        Transaction otherPeriodTransaction = GetService<AddTransactionAction>().Run(
            accountingPeriodType == AccountingPeriodType.Past ? PastAccountingPeriod : FutureAccountingPeriod,
            eventDate,
            Account,
            null,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 500.00m
                }
            ]
        );
        otherPeriodTransaction.Post(TransactionAccountType.Debit, otherPeriodTransaction.TransactionDate);
    }
}