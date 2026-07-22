namespace Domain.Funds.Queries;

/// <summary>
/// Interface for retrieving persisted Fund facts used by read operations.
/// </summary>
public interface IFundQueryRepository
{
    /// <summary>
    /// Retrieves the Funds matching the provided query.
    /// </summary>
    Task<QueryPage<Fund>> GetAsync(FundQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Funds and their current balances matching the provided query.
    /// </summary>
    Task<QueryPage<FundBalance>> GetBalancesAsync(
        FundBalanceQuery query,
        CancellationToken cancellationToken = default);
}