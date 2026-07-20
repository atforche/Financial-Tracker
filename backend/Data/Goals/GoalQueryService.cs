using Domain.AccountingPeriods;
using Domain.Goals;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.AccountingPeriods;
using Models.Funds;
using Models.Goals;

namespace Data.Goals;

/// <summary>
/// Read-only queries for Goal API models.
/// </summary>
public sealed class GoalQueryService(DatabaseContext databaseContext)
{
    /// <summary>
    /// Retrieves Assignment Goals matching the provided query.
    /// </summary>
    public async Task<CollectionModel<AssignmentGoalModel>> GetAssignmentGoalsAsync(
        AssignmentGoalQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AssignmentGoal> query = ApplyFilter(databaseContext.AssignmentGoals.AsNoTracking(), request.Filter);
        query = ApplySort(query, request.Sort);
        int totalCount = await query.CountAsync(cancellationToken);
        List<AssignmentGoal> goals = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        Dictionary<Guid, AccountingPeriodModel> periods = await GetPeriodsAsync(goals.Select(goal => goal.AccountingPeriodId?.Value), cancellationToken);
        return new CollectionModel<AssignmentGoalModel>
        {
            Items = goals.Select(goal => ToModel(goal, periods)).ToList(),
            TotalCount = totalCount,
        };
    }

    /// <summary>
    /// Retrieves Spending Goals matching the provided query.
    /// </summary>
    public async Task<CollectionModel<SpendingGoalModel>> GetSpendingGoalsAsync(
        SpendingGoalQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SpendingGoal> query = ApplyFilter(databaseContext.SpendingGoals.AsNoTracking(), request.Filter);
        query = ApplySort(query, request.Sort);
        int totalCount = await query.CountAsync(cancellationToken);
        List<SpendingGoal> goals = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        Dictionary<Guid, AccountingPeriodModel> periods = await GetPeriodsAsync(goals.Select(goal => goal.AccountingPeriodId?.Value), cancellationToken);
        return new CollectionModel<SpendingGoalModel>
        {
            Items = goals.Select(goal => ToModel(goal, periods)).ToList(),
            TotalCount = totalCount,
        };
    }

    /// <summary>
    /// Retrieves an Assignment Goal by ID.
    /// </summary>
    public async Task<AssignmentGoalModel?> GetAssignmentGoalByIdAsync(Guid goalId, CancellationToken cancellationToken = default)
    {
        AssignmentGoal? goal = await databaseContext.AssignmentGoals.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == new AssignmentGoalId(goalId), cancellationToken);
        if (goal == null)
        {
            return null;
        }
        Dictionary<Guid, AccountingPeriodModel> periods = await GetPeriodsAsync([goal.AccountingPeriodId?.Value], cancellationToken);
        return ToModel(goal, periods);
    }

    /// <summary>
    /// Retrieves a Spending Goal by ID.
    /// </summary>
    public async Task<SpendingGoalModel?> GetSpendingGoalByIdAsync(Guid goalId, CancellationToken cancellationToken = default)
    {
        SpendingGoal? goal = await databaseContext.SpendingGoals.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == new SpendingGoalId(goalId), cancellationToken);
        if (goal == null)
        {
            return null;
        }
        Dictionary<Guid, AccountingPeriodModel> periods = await GetPeriodsAsync([goal.AccountingPeriodId?.Value], cancellationToken);
        return ToModel(goal, periods);
    }

    /// <summary>
    /// Applies the filter to the provided query
    /// </summary>
    private static IQueryable<AssignmentGoal> ApplyFilter(IQueryable<AssignmentGoal> query, GoalFilterModel? filter)
    {
        if (filter?.AccountingPeriodIds is { Count: > 0 } periodIds)
        {
            var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
            query = query.Where(goal => goal.AccountingPeriodId != null && accountingPeriodIds.Contains(goal.AccountingPeriodId));
        }
        if (filter?.FundIds is { Count: > 0 } fundIds)
        {
            var domainFundIds = fundIds.Select(id => new Domain.Funds.FundId(id)).ToList();
            query = query.Where(goal => domainFundIds.Contains(goal.Fund.Id));
        }
        return query;
    }

    /// <summary>
    /// Applies the filter to the provided query
    /// </summary>
    private static IQueryable<SpendingGoal> ApplyFilter(IQueryable<SpendingGoal> query, GoalFilterModel? filter)
    {
        if (filter?.AccountingPeriodIds is { Count: > 0 } periodIds)
        {
            var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
            query = query.Where(goal => goal.AccountingPeriodId != null && accountingPeriodIds.Contains(goal.AccountingPeriodId));
        }
        if (filter?.FundIds is { Count: > 0 } fundIds)
        {
            var domainFundIds = fundIds.Select(id => new Domain.Funds.FundId(id)).ToList();
            query = query.Where(goal => domainFundIds.Contains(goal.Fund.Id));
        }
        return query;
    }

    /// <summary>
    /// Applies the sort to the provided query
    /// </summary>
    private IQueryable<AssignmentGoal> ApplySort(IQueryable<AssignmentGoal> query, AssignmentGoalSortModel? sort) => sort switch
    {
        AssignmentGoalSortModel.AccountingPeriod => OrderByAccountingPeriod(query).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.AccountingPeriodDescending => OrderByAccountingPeriodDescending(query).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.Fund => ThenByAccountingPeriod(query.OrderBy(goal => goal.Fund.Name)).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.FundDescending => ThenByAccountingPeriod(query.OrderByDescending(goal => goal.Fund.Name)).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.Type => query.OrderBy(goal => goal.AssignmentGoalType).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.TypeDescending => query.OrderByDescending(goal => goal.AssignmentGoalType).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.GoalAmount => query.OrderBy(goal => goal.GoalAmount).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.GoalAmountDescending => query.OrderByDescending(goal => goal.GoalAmount).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.TotalAmountToAssign => query.OrderBy(goal => goal.TotalAmountToAssign).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.TotalAmountToAssignDescending => query.OrderByDescending(goal => goal.TotalAmountToAssign).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.TotalAmountAssigned => query.OrderBy(goal => goal.TotalAmountAssigned).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.TotalAmountAssignedDescending => query.OrderByDescending(goal => goal.TotalAmountAssigned).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.IsMet => query.OrderBy(goal => goal.IsGoalMet).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        AssignmentGoalSortModel.IsMetDescending => query.OrderByDescending(goal => goal.IsGoalMet).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        _ => ThenByAccountingPeriod(query.OrderBy(goal => goal.Fund.Name)).ThenBy(goal => goal.Id),
    };

    /// <summary>
    /// Applies the sort to the provided query
    /// </summary>
    private IQueryable<SpendingGoal> ApplySort(IQueryable<SpendingGoal> query, SpendingGoalSortModel? sort) => sort switch
    {
        SpendingGoalSortModel.AccountingPeriod => OrderByAccountingPeriod(query).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.AccountingPeriodDescending => OrderByAccountingPeriodDescending(query).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.Fund => ThenByAccountingPeriod(query.OrderBy(goal => goal.Fund.Name)).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.FundDescending => ThenByAccountingPeriod(query.OrderByDescending(goal => goal.Fund.Name)).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.Type => query.OrderBy(goal => goal.SpendingGoalType).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.TypeDescending => query.OrderByDescending(goal => goal.SpendingGoalType).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.TotalAmountToSpend => query.OrderBy(goal => goal.TotalAmountToSpend).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.TotalAmountToSpendDescending => query.OrderByDescending(goal => goal.TotalAmountToSpend).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.TotalAmountSpent => query.OrderBy(goal => goal.TotalAmountSpent).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.TotalAmountSpentDescending => query.OrderByDescending(goal => goal.TotalAmountSpent).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.IsMet => query.OrderBy(goal => goal.IsGoalMet).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        SpendingGoalSortModel.IsMetDescending => query.OrderByDescending(goal => goal.IsGoalMet).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.Id),
        _ => ThenByAccountingPeriod(query.OrderBy(goal => goal.Fund.Name)).ThenBy(goal => goal.Id),
    };

    /// <summary>
    /// Orders the provided query by accounting period
    /// </summary>
    private IOrderedQueryable<AssignmentGoal> OrderByAccountingPeriod(IQueryable<AssignmentGoal> query) => query
        .OrderBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Year).FirstOrDefault())
        .ThenBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Month).FirstOrDefault());

    /// <summary>
    /// Orders the provided query by accounting period
    /// </summary>
    private IOrderedQueryable<AssignmentGoal> OrderByAccountingPeriodDescending(IQueryable<AssignmentGoal> query) => query
        .OrderByDescending(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Year).FirstOrDefault())
        .ThenByDescending(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Month).FirstOrDefault());

    /// <summary>
    /// Orders the provided query by accounting period
    /// </summary>
    private IOrderedQueryable<AssignmentGoal> ThenByAccountingPeriod(IOrderedQueryable<AssignmentGoal> query) => query
        .ThenBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Year).FirstOrDefault())
        .ThenBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Month).FirstOrDefault());

    /// <summary>
    /// Orders the provided query by accounting period
    /// </summary>
    private IOrderedQueryable<SpendingGoal> OrderByAccountingPeriod(IQueryable<SpendingGoal> query) => query
        .OrderBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Year).FirstOrDefault())
        .ThenBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Month).FirstOrDefault());

    /// <summary>
    /// Orders the provided query by accounting period
    /// </summary>
    private IOrderedQueryable<SpendingGoal> OrderByAccountingPeriodDescending(IQueryable<SpendingGoal> query) => query
        .OrderByDescending(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Year).FirstOrDefault())
        .ThenByDescending(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Month).FirstOrDefault());

    /// <summary>
    /// Orders the provided query by accounting period
    /// </summary>
    private IOrderedQueryable<SpendingGoal> ThenByAccountingPeriod(IOrderedQueryable<SpendingGoal> query) => query
        .ThenBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Year).FirstOrDefault())
        .ThenBy(goal => databaseContext.AccountingPeriods.Where(period => period.Id == goal.AccountingPeriodId).Select(period => period.Month).FirstOrDefault());

    /// <summary>
    /// Gets the accounting periods corresponding to the provided IDs
    /// </summary>
    private async Task<Dictionary<Guid, AccountingPeriodModel>> GetPeriodsAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
    {
        var periodIds = ids.Where(id => id.HasValue).Select(id => new AccountingPeriodId(id!.Value)).Distinct().ToList();
        return await databaseContext.AccountingPeriods.AsNoTracking().Where(period => periodIds.Contains(period.Id))
            .Select(period => new AccountingPeriodModel
            {
                Id = period.Id.Value,
                Name = period.Name,
                Year = period.Year,
                Month = period.Month,
                IsOpen = period.IsOpen,
            }).ToDictionaryAsync(period => period.Id, cancellationToken);
    }

    /// <summary>
    /// Converts a Assignment Goal domain object to the Assignment Goal Model
    /// </summary>
    private static AssignmentGoalModel ToModel(AssignmentGoal goal, Dictionary<Guid, AccountingPeriodModel> periods) => new()
    {
        Id = goal.Id.Value,
        Fund = new FundModel { Id = goal.Fund.Id.Value, Name = goal.Fund.Name, Description = goal.Fund.Description },
        AccountingPeriod = goal.AccountingPeriodId == null ? null : periods[goal.AccountingPeriodId.Value],
        Type = (AssignmentGoalTypeModel)goal.AssignmentGoalType,
        GoalAmount = goal.GoalAmount,
        TotalAmountToAssign = goal.TotalAmountToAssign,
        TotalAmountAssigned = goal.TotalAmountAssigned,
        TotalAmountAssignedIncludingPending = goal.TotalAmountAssignedIncludingPending,
        RemainingAmountToAssign = Math.Max(goal.TotalAmountToAssign - goal.TotalAmountAssigned, 0),
        RemainingAmountToAssignIncludingPending = Math.Max(goal.TotalAmountToAssign - goal.TotalAmountAssignedIncludingPending, 0),
        IsGoalMet = goal.IsGoalMet,
        IsGoalMetIncludingPending = goal.IsGoalMetIncludingPending,
    };

    /// <summary>
    /// Converts a Spending Goal domain object to the Spending Goal Model
    /// </summary>
    private static SpendingGoalModel ToModel(SpendingGoal goal, Dictionary<Guid, AccountingPeriodModel> periods) => new()
    {
        Id = goal.Id.Value,
        Fund = new FundModel { Id = goal.Fund.Id.Value, Name = goal.Fund.Name, Description = goal.Fund.Description },
        AccountingPeriod = goal.AccountingPeriodId == null ? null : periods[goal.AccountingPeriodId.Value],
        Type = (SpendingGoalTypeModel)goal.SpendingGoalType,
        TotalAmountToSpend = goal.TotalAmountToSpend,
        TotalAmountSpent = goal.TotalAmountSpent,
        TotalAmountSpentIncludingPending = goal.TotalAmountSpentIncludingPending,
        RemainingAmountToSpend = goal.TotalAmountToSpend - goal.TotalAmountSpent,
        RemainingAmountToSpendIncludingPending = goal.TotalAmountToSpend - goal.TotalAmountSpentIncludingPending,
        IsGoalMet = goal.IsGoalMet,
        IsGoalMetIncludingPending = goal.IsGoalMetIncludingPending,
    };
}