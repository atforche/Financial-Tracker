namespace Models.Goals;

/// <summary>
/// Model representing the query parameters that can be provided when retrieving Spending Goals
/// </summary>
public class SpendingGoalQueryParameterModel
{
    /// <summary>
    /// Accounting Period IDs to filter the Goals by
    /// </summary>
    public IReadOnlyCollection<Guid>? AccountingPeriodIds { get; init; }

    /// <summary>
    /// Fund IDs to filter the Goals by
    /// </summary>
    public IReadOnlyCollection<Guid>? FundIds { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public SpendingGoalSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}