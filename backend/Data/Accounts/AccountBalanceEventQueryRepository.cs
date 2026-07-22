using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Data.Accounts;

/// <summary>
/// Entity Framework implementation of Account balance-event fact retrieval.
/// </summary>
public sealed class AccountBalanceEventQueryRepository(DatabaseContext databaseContext) : IAccountBalanceEventQueryRepository
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
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var periodIds = ids.Select(id => new AccountingPeriodId(id)).ToList();
        return await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => periodIds.Contains(period.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountBalanceHistory>> GetAccountHistoriesAsync(
        IReadOnlyCollection<AccountId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => ids.Contains(history.Account.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
}