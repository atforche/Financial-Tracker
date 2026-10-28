namespace Models.Overview;

/// <summary>
/// Model representing the query parameters for getting the overview.
/// </summary>
public class OverviewQueryParameterModel
{
    /// <summary>
    /// Date the overview is shown as of, normally the viewer's current date.
    /// </summary>
    public required DateOnly AsOf { get; init; }

    /// <summary>
    /// Length of the balance history ending on the as-of date.
    /// </summary>
    public required OverviewHistoryRangeModel History { get; init; }
}
