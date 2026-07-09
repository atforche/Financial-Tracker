namespace Models.Goals;

/// <summary>
/// Model representing the query parameters for the current Goals endpoint.
/// </summary>
public class CurrentGoalsQueryParameterModel
{
    /// <summary>
    /// Optional Accounting Period to display. Defaults to the latest Accounting Period.
    /// </summary>
    public Guid? AccountingPeriodId { get; init; }

    /// <summary>
    /// Optional Fund identifiers to apply to the snapshot.
    /// </summary>
    public IReadOnlyCollection<Guid>? FundIds { get; init; }
}