using System.Collections;

namespace Tests.CloseAccountingPeriod.Scenarios;

/// <summary>
/// Collection class that contains all the unique Multiple Accounting Period scenarios for closing an Accounting Period
/// </summary>
public sealed class MultipleAccountingPeriodScenarios : IEnumerable<TheoryDataRow<DateOnly, DateOnly, DateOnly>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<DateOnly, DateOnly, DateOnly>> GetEnumerator()
    {
        yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly>(new DateOnly(2024, 12, 1), new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 1));
        yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly>(new DateOnly(2024, 12, 1), new DateOnly(2025, 2, 1), new DateOnly(2025, 1, 1));
        yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly>(new DateOnly(2024, 12, 1), new DateOnly(2024, 12, 1), new DateOnly(2025, 1, 1));
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="firstPeriod">First Period for this test case</param>
    /// <param name="secondPeriod">Second Period for this test case</param>
    /// <param name="thirdPeriod">Third Period for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    public static bool IsValid(DateOnly firstPeriod, DateOnly secondPeriod, DateOnly thirdPeriod)
    {
        if (firstPeriod > secondPeriod || firstPeriod > thirdPeriod || secondPeriod > thirdPeriod)
        {
            return false;
        }
        if (firstPeriod == secondPeriod || firstPeriod == thirdPeriod || secondPeriod == thirdPeriod)
        {
            return false;
        }
        if (CalculateMonthDifference(firstPeriod, secondPeriod) != 1 || CalculateMonthDifference(secondPeriod, thirdPeriod) != 1)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Calculates the number of months between two Accounting Periods
    /// </summary>
    /// <param name="firstPeriod">First Period</param>
    /// <param name="secondPeriod">Second Period</param>
    /// <returns>The number of months between the two provided Accounting Periods</returns>
    public static int CalculateMonthDifference(DateOnly firstPeriod, DateOnly secondPeriod) =>
        Math.Abs(((firstPeriod.Year - secondPeriod.Year) * 12) + firstPeriod.Month - secondPeriod.Month);
}