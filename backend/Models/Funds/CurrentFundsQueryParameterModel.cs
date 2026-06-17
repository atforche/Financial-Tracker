namespace Models.Funds;

/// <summary>
/// Model representing the query parameters for the current Funds endpoint.
/// </summary>
public class CurrentFundsQueryParameterModel
{
    /// <summary>
    /// Optional Fund Name filters to apply to the current snapshot.
    /// </summary>
    public List<string>? FundName { get; init; }
}