using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.AddAccountingPeriod.Scenarios;
using Tests.Setups;

namespace Tests.AddAccountingPeriod.Setups;

/// <summary>
/// Setup class for a <see cref="MultipleAccountingPeriodScenarios"/> for adding an Accounting Period
/// </summary>
internal sealed class MultipleAccountingPeriodScenarioSetup : ScenarioSetup
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
    /// First Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod FirstAccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="firstPeriod">First Period for this test case</param>
    public MultipleAccountingPeriodScenarioSetup(DateOnly firstPeriod)
    {
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);
        FirstAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(firstPeriod.Year, firstPeriod.Month);
        GetService<IAccountingPeriodRepository>().Add(FirstAccountingPeriod);
        Account = GetService<IAccountService>().CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
    }
}