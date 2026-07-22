using Domain;
using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.FundPlans.Queries;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;

namespace Data.FundPlans;

/// <summary>
/// Entity Framework implementation of Fund Plan read operations.
/// </summary>
public sealed class FundPlanQueryRepository(DatabaseContext databaseContext) : IFundPlanQueryRepository
{
    /// <inheritdoc/>
    public Task<FundPlan?> GetByIdAsync(FundPlanId id, CancellationToken cancellationToken = default) =>
        databaseContext.FundPlans.AsNoTracking().SingleOrDefaultAsync(plan => plan.Id == id, cancellationToken);

    /// <inheritdoc/>
    public async Task<QueryPage<FundPlan>> GetAsync(FundPlanQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<FundPlan> plans = ApplyFilter(databaseContext.FundPlans.AsNoTracking(), query.Filter);
        plans = query.Sort switch
        {
            FundPlanSort.Fund => plans.OrderBy(plan => plan.Fund.Name).ThenBy(plan => plan.Id),
            FundPlanSort.FundDescending => plans.OrderByDescending(plan => plan.Fund.Name).ThenBy(plan => plan.Id),
            _ => plans.OrderBy(plan => plan.Fund.Name).ThenBy(plan => plan.Id),
        };
        int totalCount = await plans.CountAsync(cancellationToken);
        IReadOnlyCollection<FundPlan> items = await plans.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new QueryPage<FundPlan>(items, totalCount);
    }

    /// <inheritdoc/>
    public Task<FundPlan?> GetByFundAndAccountingPeriodAsync(
        FundId fundId,
        AccountingPeriodId? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        databaseContext.FundPlans.AsNoTracking().SingleOrDefaultAsync(plan =>
            plan.Fund.Id == fundId && (plan.AccountingPeriod == null
                ? accountingPeriodId == null
                : plan.AccountingPeriod.Id == accountingPeriodId), cancellationToken);

    /// <summary>
    /// Applies the provided filter to the queryable collection of Fund Plans.
    /// </summary>
    private static IQueryable<FundPlan> ApplyFilter(IQueryable<FundPlan> query, FundPlanFilter filter)
    {
        if (filter.FundIds.Count > 0)
        {
            var fundIds = filter.FundIds.Select(id => new FundId(id)).ToList();
            query = query.Where(plan => fundIds.Contains(plan.Fund.Id));
        }
        if (filter.AccountingPeriodIds.Count > 0)
        {
            var accountingPeriodIds = filter.AccountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList();
            query = query.Where(plan => plan.AccountingPeriod != null && accountingPeriodIds.Contains(plan.AccountingPeriod.Id));
        }
        if (filter.IncludeOnboarded == false)
        {
            query = query.Where(plan => plan.AccountingPeriod != null);
        }
        else if (filter.IncludeOnboarded == true)
        {
            query = query.Where(plan => plan.AccountingPeriod == null);
        }
        return query;
    }
}