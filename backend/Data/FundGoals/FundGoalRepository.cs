using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.FundGoals;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;

namespace Data.FundGoals;

/// <summary>
/// Repository that allows Fund Goals to be persisted to the database
/// </summary>
public sealed class FundGoalRepository(DatabaseContext databaseContext) : IFundGoalRepository
{
    /// <inheritdoc/>
    public FundGoal GetById(FundGoalId id) => databaseContext.FundGoals.AsSplitQuery().Single(fundGoal => fundGoal.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out FundGoal? fundGoal)
    {
        fundGoal = databaseContext.FundGoals.AsSplitQuery().SingleOrDefault(fundGoal => fundGoal.Id == new FundGoalId(id))
            ?? databaseContext.FundGoals.Local.SingleOrDefault(fundGoal => fundGoal.Id == new FundGoalId(id));
        return fundGoal != null;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoal> GetAllByFund(FundId fundId) =>
        databaseContext.FundGoals.AsSplitQuery().Where(fundGoal => fundGoal.Fund.Id == fundId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoal> GetAllByAccountingPeriod(AccountingPeriodId? accountingPeriodId) =>
        databaseContext.FundGoals.AsSplitQuery().Where(fundGoal => fundGoal.AccountingPeriod == null
            ? accountingPeriodId == null
            : fundGoal.AccountingPeriod.Id == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public FundGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId) =>
        databaseContext.FundGoals.AsSplitQuery().SingleOrDefault(fundGoal => fundGoal.Fund.Id == fundId && (fundGoal.AccountingPeriod == null
            ? accountingPeriodId == null
            : fundGoal.AccountingPeriod.Id == accountingPeriodId))
        ?? databaseContext.FundGoals.Local.SingleOrDefault(fundGoal => fundGoal.Fund.Id == fundId && (fundGoal.AccountingPeriod == null
            ? accountingPeriodId == null
            : fundGoal.AccountingPeriod.Id == accountingPeriodId));

    /// <inheritdoc/>
    public bool TryAdd(FundGoal fundGoal)
    {
        if (GetByFundAndAccountingPeriod(fundGoal.Fund.Id, fundGoal.AccountingPeriod?.Id) != null)
        {
            return false;
        }
        databaseContext.Add(fundGoal);
        return true;
    }

    /// <inheritdoc/>
    public void Delete(FundGoal fundGoal) => databaseContext.Remove(fundGoal);
}
