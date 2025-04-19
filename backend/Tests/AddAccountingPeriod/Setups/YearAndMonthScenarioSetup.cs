using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.AddAccountingPeriod.Scenarios;
using Tests.Setups;

namespace Tests.AddAccountingPeriod.Setups;

/// <summary>
/// Setup class for a <see cref="YearAndMonthScenarios"/> for adding an Accounting Period
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
    /// <param name="year">Year for this test case</param>
    /// <param name="month">Month for this test case</param>
    public YearAndMonthScenarioSetup(int year, int month) =>
        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(year, month);
}