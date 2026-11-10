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
    /// Gets each Location's totals in the scoped transactions, limited to the given Locations when there are any.
    /// </summary>
    Task<IReadOnlyCollection<LocationTotals>> GetTotalsAsync(
        LocationFlowScope scope,
        IReadOnlyCollection<LocationId>? locationIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets how much each Account and each Fund paid the scoped Location.
    /// </summary>
    Task<LocationPaymentShares> GetPaymentSharesAsync(
        LocationFlowScope scope,
        CancellationToken cancellationToken = default);
}
