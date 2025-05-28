using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Domain.Transactions;
using Tests.GetAccountBalanceByAccountingPeriodTests.Scenarios;
using Tests.Setups;

namespace Tests.GetAccountBalanceByAccountingPeriodTests.Setups;

/// <summary>
/// Setup class for a <see cref="AccountingPeriodScenarios"/> for getting an Account Balance by Accounting Period 
/// </summary>
internal sealed class AccountingPeriodScenarioSetup : ScenarioSetup
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
    /// Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public AccountingPeriodScenarioSetup(AccountingPeriodScenario scenario)
    {
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);

        AccountingPeriod firstAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(firstAccountingPeriod);
        GetService<CloseAccountingPeriodAction>().Run(firstAccountingPeriod);

        AccountingPeriod secondAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(secondAccountingPeriod);

        Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, secondAccountingPeriod, secondAccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        AccountingPeriod thirdAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(thirdAccountingPeriod);

        AccountingPeriod = scenario switch
        {
            AccountingPeriodScenario.PeriodBeforeAccountWasAdded => firstAccountingPeriod,
            AccountingPeriodScenario.PeriodAccountWasAdded => secondAccountingPeriod,
            AccountingPeriodScenario.PeriodAfterAccountWasAdded => thirdAccountingPeriod,
            AccountingPeriodScenario.PriorPeriodHasPendingBalanceChanges => thirdAccountingPeriod,
            _ => throw new InvalidOperationException()
        };

        if (scenario == AccountingPeriodScenario.PriorPeriodHasPendingBalanceChanges)
        {
            Transaction transaction = GetService<AddTransactionAction>().Run(secondAccountingPeriod,
                new DateOnly(2025, 1, 15),
                Account,
                null,
                [
                    new FundAmount
                    {
                        FundId = Fund.Id,
                        Amount = 500.00m
                    }
                ]);
            GetService<ITransactionRepository>().Add(transaction);
        }
    }
}