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
        (DateOnly firstPeriod, DateOnly secondPeriod, DateOnly thirdPeriod, bool shouldClosePeriods) =
            new AddAccountingPeriod.Scenarios.MultipleAccountingPeriodScenarios()
                .DistinctBy(row => (row.Data.Item1, row.Data.Item2, row.Data.Item3))
                .Single(row => AddAccountingPeriod.Scenarios.MultipleAccountingPeriodScenarios.IsValid(row.Data.Item1, row.Data.Item2, row.Data.Item3))
                .Data;
        yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly>(firstPeriod, secondPeriod, thirdPeriod);
        yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly>(firstPeriod, thirdPeriod, secondPeriod);
        yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly>(firstPeriod, firstPeriod, secondPeriod);
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
        (Math.Abs(firstPeriod.Year - secondPeriod.Year) * 12) + Math.Abs(firstPeriod.Month - secondPeriod.Month);
}