namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing a range of dates
/// </summary>
public class DateRange
{
    /// <summary>
    /// Start Date for this Date Range
    /// </summary>
    public DateOnly StartDate { get; }

    /// <summary>
    /// Endpoint Type of the Start Date for this Date Range 
    /// </summary>
    public EndpointType StartDateType { get; }

    /// <summary>
    /// End Date for this Date Range
    /// </summary>
    public DateOnly EndDate { get; }

    /// <summary>
    /// Endpoint Type of the End Date for this Date Range
    /// </summary>
    public EndpointType EndDateType { get; }

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
        StartDate = startDate;
        EndDate = endDate;
        StartDateType = startDateType;
        EndDateType = endDateType;
        Validate();
    }

    /// <summary>
    /// Gets the list of dates for this Date Range
    /// </summary>
    /// <returns>The list of dates for this Date Range</returns>
    public IEnumerable<DateOnly> GetDates()
    {
        DateOnly startDate = StartDateType == EndpointType.Inclusive ? StartDate : StartDate.AddDays(1);
        DateOnly endDate = EndDateType == EndpointType.Inclusive ? EndDate : EndDate.AddDays(-1);
        DateOnly currentDate = startDate;
        while (currentDate <= endDate)
        {
            yield return currentDate;
        }
    }

    /// <summary>
    /// Validates the current Date Range
    /// </summary>
    private void Validate()
    {
        if (StartDate > EndDate)
        {
            throw new InvalidOperationException();
        }
        if (StartDate == EndDate &&
            (StartDateType == EndpointType.Exclusive || EndDateType == EndpointType.Exclusive))
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