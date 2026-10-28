using Domain.AccountingPeriods;
using Domain.Overview;
using Microsoft.EntityFrameworkCore;

namespace Data.Overview;

/// <summary>
/// Entity Framework implementation of overview read operations.
/// </summary>
public sealed class OverviewQueryRepository(DatabaseContext databaseContext) : IOverviewQueryRepository
{
    /// <inheritdoc/>
    public async Task<AccountingPeriod?> GetLatestOpenAccountingPeriodAsync(CancellationToken cancellationToken = default) =>
        await databaseContext.AccountingPeriods.AsNoTracking().AsSplitQuery()
            .Include(period => period.ExpectedIncomeSources)
            .Where(period => period.IsOpen)
            .OrderByDescending(period => period.Year).ThenByDescending(period => period.Month)
            .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<OverviewUnpostedEffect>> GetUnpostedEffectsAsync(CancellationToken cancellationToken = default) =>
        await databaseContext.PendingAccountBalanceEffects.AsNoTracking()
            .Join(
                databaseContext.Transactions,
                effect => effect.TransactionId,
                transaction => transaction.Id,
                (effect, transaction) => new OverviewUnpostedEffect(
                    effect.Account.Id,
                    effect.Account.Name,
                    effect.TransactionId,
                    transaction.Date,
                    effect.PendingDebitAmount,
                    effect.PendingCreditAmount))
            .ToListAsync(cancellationToken);
}
