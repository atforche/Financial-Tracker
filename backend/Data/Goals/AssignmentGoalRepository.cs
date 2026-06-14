using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;

namespace Data.Goals;

/// <summary>
/// Repository that allows Assignment Goals to be persisted to the database.
/// </summary>
public class AssignmentGoalRepository(DatabaseContext databaseContext) : IAssignmentGoalRepository
{
    #region IAssignmentGoalRepository

    /// <inheritdoc/>
    public IReadOnlyCollection<AssignmentGoal> GetAll() => databaseContext.AssignmentGoals.ToList();

    /// <inheritdoc/>
    public AssignmentGoal GetById(AssignmentGoalId id) =>
        databaseContext.AssignmentGoals.Single(goal => goal.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<AssignmentGoal> GetAllByFund(FundId fundId) =>
        databaseContext.AssignmentGoals.Where(goal => goal.Fund.Id == fundId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<AssignmentGoal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.AssignmentGoals.Where(goal => goal.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public AssignmentGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId) =>
        databaseContext.AssignmentGoals.FirstOrDefault(goal => goal.Fund.Id == fundId && goal.AccountingPeriodId == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(AssignmentGoal assignmentGoal) => databaseContext.Add(assignmentGoal);

    /// <inheritdoc/>
    public void Delete(AssignmentGoal assignmentGoal) => databaseContext.Remove(assignmentGoal);

    #endregion

    /// <summary>
    /// Attempts to get the Assignment Goal with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out AssignmentGoal? goal)
    {
        goal = databaseContext.AssignmentGoals.FirstOrDefault(goal => ((Guid)(object)goal.Id) == id);
        return goal != null;
    }

    /// <summary>
    /// Attempts to get the Assignment Goal with the specified Fund ID and Accounting Period ID
    /// </summary>
    public bool TryGetByFundAndAccountingPeriod(Guid fundId, Guid accountingPeriodId, [NotNullWhen(true)] out AssignmentGoal? goal)
    {
        goal = databaseContext.AssignmentGoals.FirstOrDefault(result => ((Guid)(object)result.Fund.Id) == fundId && ((Guid?)(object?)result.AccountingPeriodId) == accountingPeriodId);
        return goal != null;
    }
}