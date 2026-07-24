using Domain.Funds;

namespace Domain.FundPlans.Queries;

/// <summary>
/// Defines persisted facts needed for Fund Plan balance-event queries.
/// </summary>
public interface IFundPlanBalanceEventQueryRepository
{
    /// <summary>
    /// Retrieves Funds with the provided IDs.
    /// </summary>
    Task<IReadOnlyCollection<Fund>> GetFundsAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ordered Fund Plan totals histories for the provided Funds.
    /// </summary>
    Task<IReadOnlyCollection<FundPlanTotalsHistory>> GetFundPlanHistoriesAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default);
}