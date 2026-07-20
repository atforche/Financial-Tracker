using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Funds;

namespace Data.FundPlans;

/// <summary>
/// Repository that allows Transactions to be persisted to the database
/// </summary>
public sealed class FundPlanRepository(DatabaseContext databaseContext) : IFundPlanRepository
{
    /// <inheritdoc/>
    public FundPlan GetById(FundPlanId id) => databaseContext.FundPlans.Single(plan => plan.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<FundPlan> GetAllByFund(FundId fundId) =>
        databaseContext.FundPlans.Where(plan => plan.Fund.Id == fundId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundPlan> GetAllByAccountingPeriod(AccountingPeriodId? accountingPeriodId) =>
        databaseContext.FundPlans.Where(plan => plan.AccountingPeriod == null
            ? accountingPeriodId == null
            : plan.AccountingPeriod.Id == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public FundPlan? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId) =>
        databaseContext.FundPlans.SingleOrDefault(plan => plan.Fund.Id == fundId && (plan.AccountingPeriod == null
            ? accountingPeriodId == null
            : plan.AccountingPeriod.Id == accountingPeriodId))
        ?? databaseContext.FundPlans.Local.SingleOrDefault(plan => plan.Fund.Id == fundId && (plan.AccountingPeriod == null
            ? accountingPeriodId == null
            : plan.AccountingPeriod.Id == accountingPeriodId));

    /// <inheritdoc/>
    public bool TryAdd(FundPlan fundPlan)
    {
        if (GetByFundAndAccountingPeriod(fundPlan.Fund.Id, fundPlan.AccountingPeriod?.Id) != null)
        {
            return false;
        }
        databaseContext.Add(fundPlan);
        return true;
    }

    /// <inheritdoc/>
    public void Delete(FundPlan fundPlan) => databaseContext.Remove(fundPlan);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out FundPlan? fundPlan)
    {
        fundPlan = databaseContext.FundPlans.SingleOrDefault(plan => plan.Id == new FundPlanId(id));
        return fundPlan != null;
    }
}