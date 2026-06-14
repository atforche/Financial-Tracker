using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;

namespace Data.Goals;

/// <summary>
/// Repository that allows Spending Goals to be persisted to the database.
/// </summary>
public class SpendingGoalRepository(DatabaseContext databaseContext) : ISpendingGoalRepository
{
    #region ISpendingGoalRepository

    /// <inheritdoc/>
    public IReadOnlyCollection<SpendingGoal> GetAll() => databaseContext.SpendingGoals.ToList();

    /// <inheritdoc/>
    public SpendingGoal GetById(SpendingGoalId id) =>
        databaseContext.SpendingGoals.Single(goal => goal.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<SpendingGoal> GetAllByFund(FundId fundId) =>
        databaseContext.SpendingGoals.Where(goal => goal.Fund.Id == fundId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<SpendingGoal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.SpendingGoals.Where(goal => goal.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public SpendingGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId) =>
        databaseContext.SpendingGoals.FirstOrDefault(goal => goal.Fund.Id == fundId && goal.AccountingPeriodId == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(SpendingGoal spendingGoal) => databaseContext.Add(spendingGoal);

    /// <inheritdoc/>
    public void Delete(SpendingGoal spendingGoal) => databaseContext.Remove(spendingGoal);

    #endregion

    /// <summary>
    /// Attempts to get the Spending Goal with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out SpendingGoal? goal)
    {
        goal = databaseContext.SpendingGoals.FirstOrDefault(goal => ((Guid)(object)goal.Id) == id);
        return goal != null;
    }

    /// <summary>
    /// Attempts to get the Spending Goal with the specified Fund ID and Accounting Period ID
    /// </summary>
    public bool TryGetByFundAndAccountingPeriod(Guid fundId, Guid accountingPeriodId, [NotNullWhen(true)] out SpendingGoal? goal)
    {
        goal = databaseContext.SpendingGoals.FirstOrDefault(result => ((Guid)(object)result.Fund.Id) == fundId && ((Guid?)(object?)result.AccountingPeriodId) == accountingPeriodId);
        return goal != null;
    }
}