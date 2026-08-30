using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.AccountGoals.Queries;

/// <summary>
/// Service for querying Account Goals.
/// </summary>
public sealed class AccountGoalQueryService(IAccountGoalQueryRepository accountGoalQueryRepository)
{
    /// <summary>
    /// Retrieves Account Goals matching the provided query.
    /// </summary>
    public Task<QueryPage<AccountGoal>> GetAsync(AccountGoalQuery query, CancellationToken cancellationToken = default) =>
        accountGoalQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves the Account Goal with the specified ID, or null when it does not exist.
    /// </summary>
    public Task<AccountGoal?> GetByIdAsync(Guid accountGoalId, CancellationToken cancellationToken = default) =>
        accountGoalQueryRepository.GetByIdAsync(new AccountGoalId(accountGoalId), cancellationToken);

    /// <summary>
    /// Retrieves the Account Goal for the specified Account and Accounting Period, or null when it does not exist.
    /// </summary>
    public Task<AccountGoal?> GetByAccountAndAccountingPeriodAsync(
        Guid accountId,
        Guid? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        accountGoalQueryRepository.GetByAccountAndAccountingPeriodAsync(
            new AccountId(accountId),
            accountingPeriodId == null ? null : new AccountingPeriodId(accountingPeriodId.Value),
            cancellationToken);
}
