using System.Collections;
using Domain.Aggregates.AccountingPeriods;
using Domain.Services;

namespace Tests.Scenarios;

/// <summary>
/// Collection class that contains unique year and month scenarios that should be tested
/// </summary>
public sealed class YearAndMonthScenarios : IEnumerable<TheoryDataRow<int, int>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<int, int>> GetEnumerator()
    {
        List<int> yearsToTest = [0, 2025, 3000];
        var monthsToTest = Enumerable.Range(0, 14).ToList();
        foreach (int year in yearsToTest)
        {
            foreach (int month in monthsToTest)
            {
                yield return new TheoryDataRow<int, int>(year, month);
            }
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Setup class for a year and month scenario
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