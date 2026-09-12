namespace Models.Accounts;

/// <summary>
/// Model representing query parameters for an Account's balance events.
/// </summary>
public sealed class AccountBalanceEventsQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Date range for the posted balance-event history.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Restricts events to transactions assigned to this Accounting Period.
    /// </summary>
    public Guid? AccountingPeriodId { get; init; }

    /// <summary>
    /// Sort order to apply to the results.
    /// </summary>
    public AccountBalanceEventSortModel? Sort { get; init; }
}
