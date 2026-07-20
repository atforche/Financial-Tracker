using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.AccountingPeriods;
using Models.FundPlans;
using Models.Funds;

namespace Data.FundPlans;

/// <summary>
/// Read-only queries for Fund Plan API models.
/// </summary>
public sealed class FundPlanQueryService(DatabaseContext databaseContext)
{
    /// <summary>
    /// Retrieves Fund Plans matching the provided query
    /// </summary>
    public async Task<CollectionModel<FundPlanModel>> GetAsync(FundPlanQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        IQueryable<FundPlan> query = databaseContext.FundPlans.AsNoTracking();
        if (request.Filter?.FundIds is { Count: > 0 } fundIds)
        {
            var domainFundIds = fundIds.Select(id => new FundId(id)).ToList();
            query = query.Where(plan => domainFundIds.Contains(plan.Fund.Id));
        }
        if (request.Filter?.AccountingPeriodIds is { Count: > 0 } accountingPeriodIds)
        {
            var domainAccountingPeriodIds = accountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList();
            query = query.Where(plan => plan.AccountingPeriod != null && domainAccountingPeriodIds.Contains(plan.AccountingPeriod.Id));
        }
        if (request.Filter?.IncludeOnboarded == false)
        {
            query = query.Where(plan => plan.AccountingPeriod != null);
        }
        else if (request.Filter?.IncludeOnboarded == true)
        {
            query = query.Where(plan => plan.AccountingPeriod == null);
        }
        query = request.Sort == FundPlanSortModel.FundDescending
            ? query.OrderByDescending(plan => plan.Fund.Name).ThenBy(plan => plan.Id)
            : query.OrderBy(plan => plan.Fund.Name).ThenBy(plan => plan.Id);
        int totalCount = await query.CountAsync(cancellationToken);
        List<FundPlan> plans = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new CollectionModel<FundPlanModel> { Items = plans.Select(ToModel).ToList(), TotalCount = totalCount };
    }

    /// <summary>
    /// Retrieves a Fund Plan by its ID
    /// </summary>
    public async Task<FundPlanModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        ToOptionalModel(await databaseContext.FundPlans.AsNoTracking().SingleOrDefaultAsync(plan => plan.Id == new FundPlanId(id), cancellationToken));

    /// <summary>
    /// Retrieves a Fund Plan by its associated Fund ID
    /// </summary>
    public async Task<FundPlanModel?> GetByFundAndAccountingPeriodAsync(
        Guid fundId,
        Guid? accountingPeriodId,
        CancellationToken cancellationToken = default)
    {
        var domainFundId = new FundId(fundId);
        AccountingPeriodId? domainAccountingPeriodId = accountingPeriodId == null ? null : new AccountingPeriodId(accountingPeriodId.Value);
        return ToOptionalModel(await databaseContext.FundPlans.AsNoTracking().SingleOrDefaultAsync(plan =>
            plan.Fund.Id == domainFundId && (plan.AccountingPeriod == null
                ? domainAccountingPeriodId == null
                : plan.AccountingPeriod.Id == domainAccountingPeriodId), cancellationToken));
    }

    /// <summary>
    /// Converts a Fund Plan to its API model representation
    /// </summary>
    private static FundPlanModel ToModel(FundPlan plan) => new()
    {
        Id = plan.Id.Value,
        Fund = new FundModel { Id = plan.Fund.Id.Value, Name = plan.Fund.Name, Description = plan.Fund.Description },
        AccountingPeriod = plan.AccountingPeriod == null ? null : new AccountingPeriodModel
        {
            Id = plan.AccountingPeriod.Id.Value,
            Name = plan.AccountingPeriod.Name,
            Year = plan.AccountingPeriod.Year,
            Month = plan.AccountingPeriod.Month,
            IsOpen = plan.AccountingPeriod.IsOpen,
        },
        RegularContribution = plan.RegularContribution,
        MinimumFundedBalance = plan.MinimumFundedBalance,
        MaximumFundedBalance = plan.MaximumFundedBalance,
        TargetEndingBalance = plan.TargetEndingBalance,
    };

    /// <summary>
    /// Converts a Fund Plan to its API model representation, returning null if the plan is null
    /// </summary>
    private static FundPlanModel? ToOptionalModel(FundPlan? plan) => plan == null ? null : ToModel(plan);
}