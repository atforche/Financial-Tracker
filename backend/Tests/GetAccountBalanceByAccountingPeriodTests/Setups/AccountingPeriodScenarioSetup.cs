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
        Fund = GetService<IFundService>().CreateNewFund("Test");
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
                    Fund = Fund,
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
            _ => throw new InvalidOperationException()
        };
    }
}