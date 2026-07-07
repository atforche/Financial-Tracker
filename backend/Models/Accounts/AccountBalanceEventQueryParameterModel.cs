namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters for an Account balance-event collection.
/// </summary>
public class AccountBalanceEventQueryParameterModel
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