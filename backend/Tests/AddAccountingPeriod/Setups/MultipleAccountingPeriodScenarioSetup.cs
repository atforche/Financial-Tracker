using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;

namespace Tests.AddAccountingPeriod.Setups;

/// <summary>
/// Setup class for a Multiple Accounting Period scenario for adding an Accounting Period
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
    /// True if Accounting Periods should be closed before adding a new one, false otherwise
    /// </summary>
    public bool ShouldClosePeriods { get; }

    public MultipleAccountingPeriodScenarioSetup(DateOnly firstPeriod, bool shouldClosePeriod)
    {
        ShouldClosePeriods = shouldClosePeriod;
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);
        FirstAccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(firstPeriod.Year, firstPeriod.Month);
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