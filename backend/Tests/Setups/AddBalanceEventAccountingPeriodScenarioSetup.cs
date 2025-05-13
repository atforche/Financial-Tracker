using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;
using Tests.Scenarios;

namespace Tests.Setups;

/// <summary>
/// Setup class for a <see cref="AddBalanceEventAccountingPeriodScenarios"/> for adding a Balance Event
/// </summary>
internal sealed class AddBalanceEventAccountingPeriodScenarioSetup : ScenarioSetup
{
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
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public AddBalanceEventAccountingPeriodScenarioSetup(AddBalanceEventAccountingPeriodScenario scenario)
    {
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<AddFundAction>().Run("Test2");
        GetService<IFundRepository>().Add(OtherFund);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        AccountingPeriod nextAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(nextAccountingPeriod);

        Account = GetService<AddAccountAction>().Run("Test",
            AccountType.Standard,
            scenario == AddBalanceEventAccountingPeriodScenario.PriorToAccountBeingAdded ? nextAccountingPeriod : AccountingPeriod,
            AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        if (scenario == AddBalanceEventAccountingPeriodScenario.Closed)
        {
            GetService<CloseAccountingPeriodAction>().Run(AccountingPeriod);
        }
    }
}