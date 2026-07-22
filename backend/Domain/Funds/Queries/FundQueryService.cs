namespace Domain.Funds.Queries;

/// <summary>
/// Service for querying Funds and their Balances.
/// </summary>
public sealed class FundQueryService(IFundRepository fundRepository, IFundQueryRepository fundQueryRepository)
{
    /// <summary>
    /// Retrieves the Fund with the specified ID, or null when it does not exist.
    /// </summary>
    public Fund? GetById(Guid fundId)
    {
        if (fundRepository.TryGetById(fundId, out Fund? fund))
        {
            return fund;
        }
        return null;
    }

    /// <summary>
    /// Retrieves the Funds matching the provided query.
    /// </summary>
    public Task<QueryPage<Fund>> GetAsync(FundQuery query, CancellationToken cancellationToken = default) =>
        fundQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Funds and their current balances.
    /// </summary>
    public Task<QueryPage<FundBalance>> GetWithBalancesAsync(
        FundBalanceQuery query,
        CancellationToken cancellationToken = default) =>
        fundQueryRepository.GetBalancesAsync(query, cancellationToken);
}