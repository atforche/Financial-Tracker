namespace Models.Overview;

/// <summary>
/// Model representing the query parameters for getting the overview over a date range.
/// </summary>
public class OverviewDateRangeQueryParameterModel
{
    /// <summary>
    /// Range of dates to get the overview for. Both dates are required.
    /// </summary>
    public required DateRangeModel Range { get; init; }
}
