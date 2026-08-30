using Domain;
using Domain.AccountGoals;
using Domain.AccountGoals.Queries;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Data.AccountGoals;

/// <summary>
/// Entity Framework implementation of Account Goal read operations.
/// </summary>
public sealed class AccountGoalQueryRepository(DatabaseContext databaseContext) : IAccountGoalQueryRepository
{
    /// <inheritdoc/>
    public Task<AccountGoal?> GetByIdAsync(AccountGoalId id, CancellationToken cancellationToken = default) =>
        databaseContext.AccountGoals.AsNoTracking().SingleOrDefaultAsync(accountGoal => accountGoal.Id == id, cancellationToken);

    /// <inheritdoc/>
    public async Task<QueryPage<AccountGoal>> GetAsync(AccountGoalQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<AccountGoal> accountGoals = ApplyFilter(databaseContext.AccountGoals.AsNoTracking(), query.Filter);
        accountGoals = query.Sort switch
        {
            AccountGoalSort.Account => accountGoals.OrderBy(accountGoal => accountGoal.Account.Name).ThenBy(accountGoal => accountGoal.Id),
            AccountGoalSort.AccountDescending => accountGoals.OrderByDescending(accountGoal => accountGoal.Account.Name).ThenBy(accountGoal => accountGoal.Id),
            _ => accountGoals.OrderBy(accountGoal => accountGoal.Account.Name).ThenBy(accountGoal => accountGoal.Id),
        };
        int totalCount = await accountGoals.CountAsync(cancellationToken);
        IReadOnlyCollection<AccountGoal> items = await accountGoals.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new QueryPage<AccountGoal>(items, totalCount);
    }

    /// <inheritdoc/>
    public Task<AccountGoal?> GetByAccountAndAccountingPeriodAsync(
        AccountId accountId,
        AccountingPeriodId? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        databaseContext.AccountGoals.AsNoTracking().SingleOrDefaultAsync(accountGoal =>
            accountGoal.Account.Id == accountId && (accountGoal.AccountingPeriod == null
                ? accountingPeriodId == null
                : accountGoal.AccountingPeriod.Id == accountingPeriodId), cancellationToken);

    /// <summary>
    /// Applies the provided filter to the queryable collection of Account Goals.
    /// </summary>
    private static IQueryable<AccountGoal> ApplyFilter(IQueryable<AccountGoal> query, AccountGoalFilter filter)
    {
        if (filter.AccountIds.Count > 0)
        {
            var accountIds = filter.AccountIds.Select(id => new AccountId(id)).ToList();
            query = query.Where(accountGoal => accountIds.Contains(accountGoal.Account.Id));
        }
        if (filter.AccountingPeriodIds.Count > 0)
        {
            var accountingPeriodIds = filter.AccountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList();
            query = query.Where(accountGoal => accountGoal.AccountingPeriod != null
                && accountingPeriodIds.Contains(accountGoal.AccountingPeriod.Id));
        }
        if (filter.IncludeOnboarded == false)
        {
            query = query.Where(accountGoal => accountGoal.AccountingPeriod != null);
        }
        else if (filter.IncludeOnboarded == true)
        {
            query = query.Where(accountGoal => accountGoal.AccountingPeriod == null);
        }
        return query;
    }
}
