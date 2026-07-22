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

    /// <summary>
    /// Retrieves Funds matching the provided range filter.
    /// </summary>
    Task<IReadOnlyCollection<Fund>> GetRangeFundsAsync(FundFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available Fund names.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetAllNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Fund balance facts over the provided chronological range.
    /// </summary>
    Task<IReadOnlyCollection<FundPeriodBalanceFacts>> GetPeriodBalanceFactsAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);
}