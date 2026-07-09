namespace Models.Goals;

/// <summary>
/// Model representing the query parameters for a Goal balance-event collection.
/// </summary>
public class GoalBalanceEventQueryParameterModel
{
    /// <summary>
    /// Accounting Period containing the Goal.
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip.
    /// </summary>
    public int? Offset { get; init; }
}