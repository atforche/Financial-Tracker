using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.GetAccountBalanceByDateTests.Scenarios;
using Tests.Setups;

namespace Tests.GetAccountBalanceByDateTests.Setups;

/// <summary>
/// Setup class for a <see cref="AccountingPeriodOverlapScenarios"/> for getting an Account Balance by Date
/// </summary>
internal sealed class AccountingPeriodOverlapScenarioSetup : ScenarioSetup
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
    public AccountingPeriodOverlapScenarioSetup(AccountingPeriodType accountingPeriodType, DateOnly eventDate)
    {
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);

        PastAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(PastAccountingPeriod);

        Account = GetService<IAccountService>().CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        CurrentAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(CurrentAccountingPeriod);

        FutureAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(FutureAccountingPeriod);

        Transaction transaction = GetService<IAccountingPeriodService>().AddTransaction(CurrentAccountingPeriod,
            new DateOnly(2025, 1, 15),
            Account,
            null,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 250.00m
                }
            ]);
        GetService<IAccountingPeriodService>().PostTransaction(transaction, Account, transaction.TransactionDate);

        Transaction otherPeriodTransaction = GetService<IAccountingPeriodService>().AddTransaction(
            accountingPeriodType == AccountingPeriodType.Past ? PastAccountingPeriod : FutureAccountingPeriod,
            eventDate,
            Account,
            null,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 500.00m
                }
            ]
        );
        GetService<IAccountingPeriodService>().PostTransaction(otherPeriodTransaction, Account, otherPeriodTransaction.TransactionDate);
    }
}