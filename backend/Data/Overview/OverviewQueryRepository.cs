using Domain.AccountingPeriods;
using Domain.Accounts.Queries;
using Domain.Funds.Queries;
using Domain.Overview;
using Microsoft.EntityFrameworkCore;

namespace Data.Overview;

/// <summary>
/// Entity Framework implementation of overview read operations.
/// </summary>
public sealed class OverviewQueryRepository(DatabaseContext databaseContext) : IOverviewQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<OverviewAccountOpeningBalance>> GetAccountOpeningBalancesAsync(
        DateOnly beforeDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Accounts.AsNoTracking()
            .Select(account => new OverviewAccountOpeningBalance(
                account.Id,
                account.Type,
                account.DateOpened,
                databaseContext.AccountBalanceHistories
                    .Where(history => history.Account.Id == account.Id && history.Date < beforeDate)
                    .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                    .Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? account.OnboardedBalance ?? 0))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountDateBalanceFact>> GetAccountBalanceHistoryAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => history.Date >= startDate && history.Date <= endDate)
            .Select(history => new AccountDateBalanceFact(history.Account.Id, history.Date, history.Sequence, history.PostedBalance))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<OverviewFundOpeningBalance>> GetFundOpeningBalancesAsync(
        DateOnly beforeDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Funds.AsNoTracking()
            .Select(fund => new OverviewFundOpeningBalance(
                fund.Id,
                databaseContext.FundBalanceHistories
                    .Where(history => history.Fund.Id == fund.Id && history.Date < beforeDate)
                    .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                    .Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? fund.OnboardedBalance ?? 0))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FundDateBalanceFact>> GetFundBalanceHistoryAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => history.Date >= startDate && history.Date <= endDate)
            .Select(history => new FundDateBalanceFact(history.Fund.Id, history.Date, history.Sequence, history.PostedBalance))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<AccountingPeriodId?> GetLatestOpenAccountingPeriodIdAsync(CancellationToken cancellationToken = default) =>
        await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => period.IsOpen)
            .OrderByDescending(period => period.Year).ThenByDescending(period => period.Month)
            .Select(period => period.Id)
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
