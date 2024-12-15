namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing a range of dates
/// </summary>
public class DateRange
{
    private DateOnly _startDate { get; }
    private EndpointType _startDateType { get; }
    private DateOnly _endDate { get; }
    private EndpointType _endDateType { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="startDate">Start Date for this Date Range</param>
    /// <param name="endDate">End Date for this Date Range</param>
    /// <param name="startDateType">Endpoint Type of the Start Date</param>
    /// <param name="endDateType">Endpoint Type of the End Date</param>
    public DateRange(DateOnly startDate,
        DateOnly endDate,
        EndpointType startDateType = EndpointType.Inclusive,
        EndpointType endDateType = EndpointType.Inclusive)
    {
        _startDate = startDate;
        _endDate = endDate;
        _startDateType = startDateType;
        _endDateType = endDateType;
        Validate();
    }

    /// <summary>
    /// Gets the list of inclusive dates for this Date Range
    /// </summary>
    /// <returns>The list of dates for this Date Range</returns>
    public IEnumerable<DateOnly> GetInclusiveDates()
    {
        DateOnly startDate = _startDateType == EndpointType.Inclusive ? _startDate : _startDate.AddDays(1);
        DateOnly endDate = _endDateType == EndpointType.Inclusive ? _endDate : _endDate.AddDays(-1);
        DateOnly currentDate = startDate;
        while (currentDate <= endDate && currentDate < DateOnly.MaxValue)
        {
            yield return currentDate;
            currentDate = currentDate.AddDays(1);
        }
    }

    /// <summary>
    /// Determines if the provided date falls on or after the start date of the current Date Range
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <returns>True if the provided date falls on or after the start date of the current Date Range</returns>
    public bool IsWithinStartDate(DateOnly date) =>
        _startDateType == EndpointType.Inclusive ? date >= _startDate : date > _startDate;

    /// <summary>
    /// Determines if the provided date falls before or on the end date of the current Date Range
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <returns>True if the provided date falls before or on the end date of this date range, false otherwise</returns>
    public bool IsWithinEndDate(DateOnly date) =>
        _endDateType == EndpointType.Inclusive ? date <= _endDate : date < _endDate;

    /// <summary>
    /// Determines if the provided date falls within the current Date Range
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <returns>True if the provided date falls in the date range, false otherwise</returns>
    public bool IsInRange(DateOnly date) => IsWithinStartDate(date) && IsWithinEndDate(date);

    /// <summary>
    /// Validates the current Date Range
    /// </summary>
    private void Validate()
    {
        if (!GetInclusiveDates().Any())
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Type of the Endpoint for a Date Range
/// </summary>
public enum EndpointType
{
    /// <summary>
    /// Endpoint date should be included in the Date Range
    /// </summary>
    Inclusive,

    /// <summary>
    /// Endpoint date should be excluded from the Date Range
    /// </summary>
    Exclusive,
}