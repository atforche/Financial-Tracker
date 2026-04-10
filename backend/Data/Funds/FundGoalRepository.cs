using Domain.AccountingPeriods;
using Domain.Funds;

namespace Data.Funds;

/// <summary>
/// Repository that allows Fund Goals to be persisted to the database
/// </summary>
public class FundGoalRepository(DatabaseContext databaseContext) : IFundGoalRepository
{
    #region IFundGoalRepository

    /// <inheritdoc/>
    public FundGoal GetById(FundGoalId id) =>
        databaseContext.FundGoals.Single(fundGoal => fundGoal.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoal> GetAllByFund(FundId fundId) =>
        databaseContext.FundGoals.Where(fundGoal => fundGoal.Fund.Id == fundId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.FundGoals.Where(fundGoal => fundGoal.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public FundGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        databaseContext.FundGoals.FirstOrDefault(fundGoal =>
            fundGoal.Fund.Id == fundId &&
            fundGoal.AccountingPeriodId == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(FundGoal fundGoal) => databaseContext.Add(fundGoal);

    /// <inheritdoc/>
    public void Delete(FundGoal fundGoal) => databaseContext.Remove(fundGoal);

    #endregion
}
