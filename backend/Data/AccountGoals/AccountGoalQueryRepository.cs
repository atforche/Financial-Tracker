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
    public Task<AccountGoal?> GetByAccountAndAccountingPeriodAsync(
        AccountId accountId,
        AccountingPeriodId? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        databaseContext.AccountGoals.AsNoTracking().SingleOrDefaultAsync(accountGoal =>
            accountGoal.Account.Id == accountId && (accountGoal.AccountingPeriod == null
                ? accountingPeriodId == null
                : accountGoal.AccountingPeriod.Id == accountingPeriodId), cancellationToken);
}
