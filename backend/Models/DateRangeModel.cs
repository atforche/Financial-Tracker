namespace Models;

/// <summary>
/// Model representing a range of dates.
/// </summary>
public class DateRangeModel
{
    /// <summary>
    /// Start date of the range.
    /// </summary>
    public DateOnly? Start { get; init; }

    /// <summary>
    /// End date of the range.
    /// </summary>
    public DateOnly? End { get; init; }
}
