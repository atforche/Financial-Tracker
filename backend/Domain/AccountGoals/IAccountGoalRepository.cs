using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.AccountGoals;

/// <summary>
/// Interface representing methods to interact with Account Goals.
/// </summary>
public interface IAccountGoalRepository
{
    /// <summary>
    /// Gets the Account Goal with the specified ID.
    /// </summary>
    AccountGoal GetById(AccountGoalId id);

    /// <summary>
    /// Attempts to get the Account Goal with the specified ID.
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out AccountGoal? accountGoal);

    /// <summary>
    /// Gets all Account Goals associated with the specified Account.
    /// </summary>
    IReadOnlyCollection<AccountGoal> GetAllByAccount(AccountId accountId);

    /// <summary>
    /// Gets all Account Goals associated with the specified Accounting Period.
    /// </summary>
    IReadOnlyCollection<AccountGoal> GetAllByAccountingPeriod(AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Attempts to get the Account Goal associated with the specified Account and Accounting Period.
    /// </summary>
    AccountGoal? GetByAccountAndAccountingPeriod(AccountId accountId, AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Atomically attempts to add the provided Account Goal when no Account Goal exists for its Account and Accounting Period.
    /// </summary>
    bool TryAdd(AccountGoal accountGoal);

    /// <summary>
    /// Deletes the provided Account Goal from the repository.
    /// </summary>
    void Delete(AccountGoal accountGoal);
}
