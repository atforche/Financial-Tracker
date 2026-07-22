using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.FundPlans.Queries;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Data.FundPlans;

/// <summary>
/// Entity Framework implementation of Fund Plan balance-event fact retrieval.
/// </summary>
public sealed class FundPlanBalanceEventQueryRepository(DatabaseContext databaseContext) : IFundPlanBalanceEventQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Transactions.AsNoTracking()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountingPeriod>> GetAccountingPeriodsAsync(
        IReadOnlyCollection<AccountingPeriodId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => ids.Contains(period.Id))
            .ToListAsync(cancellationToken);

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