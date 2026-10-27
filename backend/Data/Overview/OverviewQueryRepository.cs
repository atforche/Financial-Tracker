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
    public async Task<IReadOnlyCollection<OverviewAccountPeriodBalance>> GetAccountPeriodBalancesAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .SelectMany(history => history.AccountBalances)
            .Where(balance => accountingPeriodIds.Contains(balance.AccountingPeriod.Id))
            .Select(balance => new OverviewAccountPeriodBalance(
                balance.AccountingPeriod.Id,
                balance.Account.Type,
                balance.OpeningBalance,
                balance.ClosingBalance))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<OverviewFundPeriodBalance>> GetFundPeriodBalancesAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .SelectMany(history => history.FundBalances)
            .Where(balance => accountingPeriodIds.Contains(balance.AccountingPeriod.Id))
            .Select(balance => new OverviewFundPeriodBalance(
                balance.AccountingPeriod.Id,
                balance.Fund.Id,
                balance.OpeningBalance,
                balance.ClosingBalance))
            .ToListAsync(cancellationToken);
}
