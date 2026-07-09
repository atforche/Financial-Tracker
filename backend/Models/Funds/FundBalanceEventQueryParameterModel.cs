namespace Models.Funds;

/// <summary>
/// Model representing the query parameters for a Fund balance-event collection.
/// </summary>
public class FundBalanceEventQueryParameterModel
{
    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip.
    /// </summary>
    public int? Offset { get; init; }
}