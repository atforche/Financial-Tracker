using System.Collections;

namespace Tests.Old.Scenarios;

/// <summary>
/// Collection class that contains all the unique Event Date scenarios for adding a Balance Event
/// </summary>
public sealed class AddBalanceEventDateScenarios : IEnumerable<TheoryDataRow<DateOnly>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<DateOnly>> GetEnumerator() => new List<TheoryDataRow<DateOnly>>
        {
            new(new DateOnly(2024, 11, 1)), // Too many months in past
            new(new DateOnly(2024, 12, 1)), // Date before the Account was added
            new(new DateOnly(2024, 12, 20)),
            new(new DateOnly(2025, 1, 1)),
            new(new DateOnly(2025, 2, 1)),
            new(new DateOnly(2025, 3, 1)), // Too many months in future
        }.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="eventDate">Event Date for this test case</param>
    /// <returns>True if the provided scenario is valid, false otherwise</returns>
    public static bool IsValid(DateOnly eventDate)
    {
        if (eventDate < new DateOnly(2024, 12, 15))
        {
            // Event date is earlier than the Account was added
            return false;
        }
        return CalculateMonthDifference(eventDate, new DateOnly(2025, 1, 1)) <= 1;
    }

    /// <summary>
    /// Calculates the number of months between the provided Event Date and Accounting Period
    /// </summary>
    /// <param name="eventDate">Event Date for this scenario</param>
    /// <param name="accountingPeriodDate">Accounting Period for this scenario</param>
    /// <returns>The number of months between the current Accounting Period and the event date</returns>
    private static int CalculateMonthDifference(DateOnly eventDate, DateOnly accountingPeriodDate) =>
        Math.Abs(((eventDate.Year - accountingPeriodDate.Year) * 12) + eventDate.Month - accountingPeriodDate.Month);
}