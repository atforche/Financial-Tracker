using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.Scenarios;

namespace Tests.AddAccountingPeriod.Setups;

/// <summary>
/// Setup class for a Year and Month scenario for adding an Accounting Period
/// </summary>
internal sealed class YearAndMonthScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Accounting Period for this Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="year">Year for this Setup</param>
    /// <param name="month">Month for this Setup</param>
    public YearAndMonthScenarioSetup(int year, int month) =>
        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(year, month);
}