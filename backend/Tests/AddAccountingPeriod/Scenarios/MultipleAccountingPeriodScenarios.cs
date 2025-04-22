using System.Collections;

namespace Tests.AddAccountingPeriod.Scenarios;

/// <summary>
/// Collection class that contains all the unique Multiple Accounting Period scenarios for adding an Accounting Period
/// </summary>
public sealed class MultipleAccountingPeriodScenarios : IEnumerable<TheoryDataRow<DateOnly, DateOnly, DateOnly, bool>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<DateOnly, DateOnly, DateOnly, bool>> GetEnumerator()
    {
        List<(DateOnly, DateOnly, DateOnly)> scenarios =
        [
            (new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 1), new DateOnly(2025, 3, 1)),
            (new DateOnly(2025, 1, 1), new DateOnly(2025, 3, 1), new DateOnly(2025, 4, 1)),
            (new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 1)),
            (new DateOnly(2025, 1, 1), new DateOnly(2024, 11, 1), new DateOnly(2025, 2, 1))
        ];
        foreach ((DateOnly firstPeriod, DateOnly secondPeriod, DateOnly thirdPeriod) in scenarios)
        {
            yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly, bool>(firstPeriod, secondPeriod, thirdPeriod, false);
            yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly, bool>(firstPeriod, secondPeriod, thirdPeriod, true);
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="firstPeriod">First Period for this scenario</param>
    /// <param name="secondPeriod">Second Period for this scenario</param>
    /// <param name="thirdPeriod">Third Period for this scenario</param>
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