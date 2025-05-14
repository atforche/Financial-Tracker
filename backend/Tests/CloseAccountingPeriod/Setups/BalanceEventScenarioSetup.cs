using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Tests.CloseAccountingPeriod.Scenarios;
using Tests.Setups;

namespace Tests.CloseAccountingPeriod.Setups;

/// <summary>
/// Setup class for a <see cref="BalanceEventScenarios"/> for closing an Accounting Period
/// </summary>
internal sealed class BalanceEventScenarioSetup : ScenarioSetup
{
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
    /// First Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public BalanceEventScenarioSetup(BalanceEventScenario scenario)
    {
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<AddFundAction>().Run("Test2");
        GetService<IFundRepository>().Add(OtherFund);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, AccountingPeriod, AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    Fund = OtherFund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        if (scenario is BalanceEventScenario.UnpostedTransaction or BalanceEventScenario.PostedTransaction)
        {
            GetService<AddTransactionAction>().Run(AccountingPeriod,
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
        }
        if (scenario is BalanceEventScenario.PostedTransaction)
        {
            AccountingPeriod.Transactions.First().Post(TransactionAccountType.Debit, new DateOnly(2025, 1, 15));
        }
        if (scenario is BalanceEventScenario.ChangeInValue)
        {
            GetService<AddChangeInValueAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 15),
                Account,
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 250.00m
                });
        }
        if (scenario is BalanceEventScenario.FundConversion)
        {
            GetService<AddFundConversionAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 15),
                Account,
                Fund,
                OtherFund,
                250.00m);
        }
    }
}