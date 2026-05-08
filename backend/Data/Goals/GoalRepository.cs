using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;

namespace Data.Goals;

/// <summary>
/// Repository that allows Goals to be persisted to the database
/// </summary>
public class GoalRepository(DatabaseContext databaseContext) : IGoalRepository
{
    #region IGoalRepository

    /// <inheritdoc/>
    public Goal GetById(GoalId id) =>
        databaseContext.Goals.Single(goal => goal.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Goal> GetAllByFund(FundId fundId) =>
        databaseContext.Goals.Where(goal => goal.Fund.Id == fundId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Goal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Goals.Where(goal => goal.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public Goal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        databaseContext.Goals.FirstOrDefault(goal =>
            goal.Fund.Id == fundId &&
            goal.AccountingPeriodId == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(Goal goal) => databaseContext.Add(goal);

    /// <inheritdoc/>
    public void Delete(Goal goal) => databaseContext.Remove(goal);

    #endregion

    /// <summary>
    /// Attempts to get the Goal with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Goal? goal)
    {
        goal = databaseContext.Goals.FirstOrDefault(goal => ((Guid)(object)goal.Id) == id);
        return goal != null;
    }

    /// <summary>
    /// Attempts to get the Goal with the specified Fund ID and Accounting Period ID
    /// </summary>
    public bool TryGetByFundAndAccountingPeriod(Guid fundId, Guid accountingPeriodId, [NotNullWhen(true)] out Goal? goal)
    {
        goal = databaseContext.Goals.FirstOrDefault(result => ((Guid)(object)result.Fund.Id) == fundId && ((Guid)(object)result.AccountingPeriodId) == accountingPeriodId);
        return goal != null;
    }
}