using System.Collections;

namespace Tests.AddAccountingPeriod.Scenarios;

/// <summary>
/// Collection class that contains all the unique Year and Month scenarios for adding an Accounting Period
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

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="year">Year for this test case</param>
    /// <param name="month">Month for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    public static bool IsValid(int year, int month) => month is > 0 and < 13 && year is > 2000 and < 2100;
}