using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.AccountGoals.Queries;

/// <summary>
/// Defines read-only persistence operations for Account Goals.
/// </summary>
public interface IAccountGoalQueryRepository
{
    /// <summary>
    /// Retrieves the Account Goal with the specified ID, or null when it does not exist.
    /// </summary>
    Task<AccountGoal?> GetByIdAsync(AccountGoalId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Account Goals matching the provided query.
    /// </summary>
    Task<QueryPage<AccountGoal>> GetAsync(AccountGoalQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Account Goal for the specified Account and Accounting Period, or null when it does not exist.
    /// </summary>
    Task<AccountGoal?> GetByAccountAndAccountingPeriodAsync(
        AccountId accountId,
        AccountingPeriodId? accountingPeriodId,
        CancellationToken cancellationToken = default);
}
