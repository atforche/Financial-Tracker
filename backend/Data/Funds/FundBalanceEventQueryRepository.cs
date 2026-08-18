using Domain.Funds;
using Domain.Funds.Queries;
using Microsoft.EntityFrameworkCore;

namespace Data.Funds;

/// <summary>
/// Entity Framework implementation of Fund balance-event fact retrieval.
/// </summary>
public sealed class FundBalanceEventQueryRepository(DatabaseContext databaseContext) : IFundBalanceEventQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Fund>> GetFundsAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Funds.AsNoTracking()
            .Where(fund => ids.Contains(fund.Id))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FundBalanceHistory>> GetFundHistoriesAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => ids.Contains(history.Fund.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
}
