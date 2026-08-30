using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.AccountGoals;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Data.AccountGoals;

/// <summary>
/// Repository that allows Account Goals to be persisted to the database.
/// </summary>
public sealed class AccountGoalRepository(DatabaseContext databaseContext) : IAccountGoalRepository
{
    /// <inheritdoc/>
    public AccountGoal GetById(AccountGoalId id) =>
        databaseContext.AccountGoals.AsSplitQuery().Single(accountGoal => accountGoal.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountGoal? accountGoal)
    {
        AccountGoalId accountGoalId = new(id);
        accountGoal = databaseContext.AccountGoals.AsSplitQuery().SingleOrDefault(item => item.Id == accountGoalId)
            ?? databaseContext.AccountGoals.Local.SingleOrDefault(item => item.Id == accountGoalId);
        return accountGoal != null;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountGoal> GetAllByAccount(AccountId accountId) =>
        databaseContext.AccountGoals.AsSplitQuery().Where(accountGoal => accountGoal.Account.Id == accountId).ToList()
            .Concat(databaseContext.AccountGoals.Local.Where(accountGoal => accountGoal.Account.Id == accountId))
            .DistinctBy(accountGoal => accountGoal.Id)
            .ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountGoal> GetAllByAccountingPeriod(AccountingPeriodId? accountingPeriodId) =>
        databaseContext.AccountGoals.AsSplitQuery().Where(accountGoal => accountGoal.AccountingPeriod == null
            ? accountingPeriodId == null
            : accountGoal.AccountingPeriod.Id == accountingPeriodId).ToList()
            .Concat(databaseContext.AccountGoals.Local.Where(accountGoal => accountGoal.AccountingPeriod == null
                ? accountingPeriodId == null
                : accountGoal.AccountingPeriod.Id == accountingPeriodId))
            .DistinctBy(accountGoal => accountGoal.Id)
            .ToList();

    /// <inheritdoc/>
    public AccountGoal? GetByAccountAndAccountingPeriod(AccountId accountId, AccountingPeriodId? accountingPeriodId) =>
        databaseContext.AccountGoals.AsSplitQuery().SingleOrDefault(accountGoal => accountGoal.Account.Id == accountId && (accountGoal.AccountingPeriod == null
            ? accountingPeriodId == null
            : accountGoal.AccountingPeriod.Id == accountingPeriodId))
        ?? databaseContext.AccountGoals.Local.SingleOrDefault(accountGoal => accountGoal.Account.Id == accountId && (accountGoal.AccountingPeriod == null
            ? accountingPeriodId == null
            : accountGoal.AccountingPeriod.Id == accountingPeriodId));

    /// <inheritdoc/>
    public bool TryAdd(AccountGoal accountGoal)
    {
        if (GetByAccountAndAccountingPeriod(accountGoal.Account.Id, accountGoal.AccountingPeriod?.Id) != null)
        {
            return false;
        }
        databaseContext.Add(accountGoal);
        return true;
    }

    /// <inheritdoc/>
    public void Delete(AccountGoal accountGoal) => databaseContext.Remove(accountGoal);
}
