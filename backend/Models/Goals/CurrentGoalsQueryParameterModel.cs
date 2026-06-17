namespace Models.Goals;

/// <summary>
/// Model representing the query parameters for the current Goals endpoint.
/// </summary>
public class CurrentGoalsQueryParameterModel
{
    /// <summary>
    /// Optional Fund Name filters to apply to the current snapshot.
    /// </summary>
    public IReadOnlyCollection<string>? FundName { get; init; }
}