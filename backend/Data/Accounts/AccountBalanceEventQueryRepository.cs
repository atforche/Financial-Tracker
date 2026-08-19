using Domain.Accounts;
using Domain.Accounts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Data.Accounts;

/// <summary>
/// Entity Framework implementation of Account balance-event fact retrieval.
/// </summary>
public sealed class AccountBalanceEventQueryRepository(DatabaseContext databaseContext) : IAccountBalanceEventQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountBalanceHistory>> GetAccountHistoriesAsync(
        IReadOnlyCollection<AccountId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => ids.Contains(history.Account.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
}
