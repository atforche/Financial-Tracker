using Domain.FundPlans;
using Domain.FundPlans.Queries;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;

namespace Data.FundPlans;

/// <summary>
/// Entity Framework implementation of Fund Plan balance-event fact retrieval.
/// </summary>
public sealed class FundPlanBalanceEventQueryRepository(DatabaseContext databaseContext) : IFundPlanBalanceEventQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Fund>> GetFundsAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Funds.AsNoTracking()
            .Where(fund => ids.Contains(fund.Id))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FundPlanTotalsHistory>> GetFundPlanHistoriesAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.FundPlanTotalsHistories.AsNoTracking()
            .Where(history => ids.Contains(history.FundId))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
}