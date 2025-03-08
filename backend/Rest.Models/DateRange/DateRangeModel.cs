using Domain.ValueObjects;

namespace Rest.Models.DateRange;

/// <summary>
/// REST model representing a Date Range
/// </summary>
public class DateRangeModel
{
    /// <summary>
    /// Start Date for this Date Range
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// Start Date Type for this Date Range
    /// </summary>
    public required EndpointType StartDateType { get; init; }

    /// <summary>
    /// End Date for this Date Range
    /// </summary>
    public required DateOnly EndDate { get; init; }

    /// <summary>
    /// End Date Type for this Date Range
    /// </summary>
    public required EndpointType EndDateType { get; init; }

    /// <summary>
    /// Gets a Date Range corresponding to this Date Range Model
    /// </summary>
    /// <returns>A Date Range corresponding to this Date Range Model</returns>
    public Domain.ValueObjects.DateRange ConvertToDateRange() => new Domain.ValueObjects.DateRange(StartDate, EndDate, StartDateType, EndDateType);
}