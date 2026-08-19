using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;

namespace Data.FundGoals;

/// <summary>
/// Entity Framework implementation of Fund Goal balance-event fact retrieval.
/// </summary>
public sealed class FundGoalBalanceEventQueryRepository(DatabaseContext databaseContext) : IFundGoalBalanceEventQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Fund>> GetFundsAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Funds.AsNoTracking()
            .Where(fund => ids.Contains(fund.Id))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FundGoalTotalsHistory>> GetFundGoalHistoriesAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.FundGoalTotalsHistories.AsNoTracking()
            .Where(history => ids.Contains(history.FundId))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
}
