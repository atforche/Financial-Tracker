namespace Domain.Locations.Queries;

/// <summary>
/// Reads Locations and the money that moved to and from them.
/// </summary>
public interface ILocationQueryRepository
{
    /// <summary>
    /// Gets a page of Locations.
    /// </summary>
    Task<QueryPage<Location>> GetAsync(LocationQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the money that moved to and from Locations in the scoped transactions.
    /// </summary>
    Task<IReadOnlyCollection<LocationFlow>> GetFlowsAsync(LocationFlowScope scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the payments made to the scoped Location, with their Accounts and Funds.
    /// </summary>
    Task<IReadOnlyCollection<LocationPayment>> GetPaymentsAsync(
        LocationFlowScope scope,
        CancellationToken cancellationToken = default);
}
