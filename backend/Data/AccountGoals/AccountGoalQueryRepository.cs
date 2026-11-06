using Domain.AccountGoals;
using Domain.AccountGoals.Queries;
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

}
