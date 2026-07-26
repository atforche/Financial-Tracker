using Domain;
using Domain.AccountingPeriods;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;

namespace Data.FundGoals;

/// <summary>
/// Entity Framework implementation of Fund Goal read operations.
/// </summary>
public sealed class FundGoalQueryRepository(DatabaseContext databaseContext) : IFundGoalQueryRepository
{
    /// <inheritdoc/>
    public Task<FundGoal?> GetByIdAsync(FundGoalId id, CancellationToken cancellationToken = default) =>
        databaseContext.FundGoals.AsNoTracking().SingleOrDefaultAsync(fundGoal => fundGoal.Id == id, cancellationToken);

    /// <inheritdoc/>
    public async Task<QueryPage<FundGoal>> GetAsync(FundGoalQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<FundGoal> fundGoals = ApplyFilter(databaseContext.FundGoals.AsNoTracking(), query.Filter);
        fundGoals = query.Sort switch
        {
            FundGoalSort.Fund => fundGoals.OrderBy(fundGoal => fundGoal.Fund.Name).ThenBy(fundGoal => fundGoal.Id),
            FundGoalSort.FundDescending => fundGoals.OrderByDescending(fundGoal => fundGoal.Fund.Name).ThenBy(fundGoal => fundGoal.Id),
            _ => fundGoals.OrderBy(fundGoal => fundGoal.Fund.Name).ThenBy(fundGoal => fundGoal.Id),
        };
        int totalCount = await fundGoals.CountAsync(cancellationToken);
        IReadOnlyCollection<FundGoal> items = await fundGoals.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new QueryPage<FundGoal>(items, totalCount);
    }

    /// <inheritdoc/>
    public Task<FundGoal?> GetByFundAndAccountingPeriodAsync(
        FundId fundId,
        AccountingPeriodId? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        databaseContext.FundGoals.AsNoTracking().SingleOrDefaultAsync(fundGoal =>
            fundGoal.Fund.Id == fundId && (fundGoal.AccountingPeriod == null
                ? accountingPeriodId == null
                : fundGoal.AccountingPeriod.Id == accountingPeriodId), cancellationToken);

    /// <summary>
    /// Applies the provided filter to the queryable collection of Fund Goals.
    /// </summary>
    private static IQueryable<FundGoal> ApplyFilter(IQueryable<FundGoal> query, FundGoalFilter filter)
    {
        if (filter.FundIds.Count > 0)
        {
            var fundIds = filter.FundIds.Select(id => new FundId(id)).ToList();
            query = query.Where(fundGoal => fundIds.Contains(fundGoal.Fund.Id));
        }
        if (filter.AccountingPeriodIds.Count > 0)
        {
            var accountingPeriodIds = filter.AccountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList();
            query = query.Where(fundGoal => fundGoal.AccountingPeriod != null && accountingPeriodIds.Contains(fundGoal.AccountingPeriod.Id));
        }
        if (filter.IncludeOnboarded == false)
        {
            query = query.Where(fundGoal => fundGoal.AccountingPeriod != null);
        }
        else if (filter.IncludeOnboarded == true)
        {
            query = query.Where(fundGoal => fundGoal.AccountingPeriod == null);
        }
        return query;
    }
}