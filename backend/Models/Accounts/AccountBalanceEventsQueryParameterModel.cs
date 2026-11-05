namespace Models.Accounts;

/// <summary>
/// Model representing query parameters for an Account's balance events.
/// </summary>
public sealed class AccountBalanceEventsQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Posting date range, used when AccountingPeriodIds is absent.
    /// </summary>
    public DateRangeModel? Range { get; init; }

    /// <summary>
    /// Restricts events to transactions assigned to these Accounting Periods.
    /// </summary>
    public List<Guid>? AccountingPeriodIds { get; init; }

    /// <summary>
    /// Whether to include current pending events in addition to matching posted events.
    /// </summary>
    public bool IncludePending { get; init; }

    /// <summary>
    /// Sort order to apply to the results.
    /// </summary>
    public AccountBalanceEventSortModel? Sort { get; init; }
}
