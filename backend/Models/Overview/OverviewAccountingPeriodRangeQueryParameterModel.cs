namespace Models.Overview;

/// <summary>
/// Model representing the query parameters for getting the overview over an Accounting Period range.
/// </summary>
public class OverviewAccountingPeriodRangeQueryParameterModel
{
    /// <summary>
    /// Range of Accounting Periods to get the overview for.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }
}
