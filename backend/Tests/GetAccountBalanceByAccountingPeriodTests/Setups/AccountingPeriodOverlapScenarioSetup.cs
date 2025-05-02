using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.GetAccountBalanceByAccountingPeriodTests.Scenarios;
using Tests.Setups;

namespace Tests.GetAccountBalanceByAccountingPeriodTests.Setups;

/// <summary>
/// Setup class for a <see cref="AccountingPeriodOverlapScenarios"/> for getting an Account Balance by Accounting Period
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
    /// <param name="eventDate">Event Date for this test case</param>
    public AccountingPeriodOverlapScenarioSetup(DateOnly eventDate)
    {
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);

        PastAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(PastAccountingPeriod);

        CurrentAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(CurrentAccountingPeriod);

        FutureAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(FutureAccountingPeriod);

        Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, PastAccountingPeriod, PastAccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        Transaction transaction = GetService<AddTransactionAction>().Run(CurrentAccountingPeriod,
            eventDate,
            Account,
            null,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 250.00m
                }
            ]);
        transaction.Post(TransactionAccountType.Debit, eventDate);
    }
}