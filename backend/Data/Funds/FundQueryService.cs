using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Funds;

namespace Data.Funds;

/// <summary>
/// Read-only queries for Fund API models.
/// </summary>
public sealed class FundQueryService(DatabaseContext databaseContext)
{
    /// <summary>
    /// Retrieves Funds matching the provided query.
    /// </summary>
    public async Task<CollectionModel<FundModel>> GetAsync(FundQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        IQueryable<Fund> query = databaseContext.Funds.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Filter?.NameSearch))
        {
            query = query.Where(fund => fund.Name.Contains(request.Filter.NameSearch));
        }
        if (request.Filter?.Names is { Count: > 0 } names)
        {
            query = query.Where(fund => names.Contains(fund.Name));
        }
        query = request.Sort switch
        {
            FundSortModel.Name => query.OrderBy(fund => fund.Name).ThenBy(fund => fund.Id),
            FundSortModel.NameDescending => query.OrderByDescending(fund => fund.Name).ThenBy(fund => fund.Id),
            FundSortModel.Description => query.OrderBy(fund => fund.Description).ThenBy(fund => fund.Name).ThenBy(fund => fund.Id),
            FundSortModel.DescriptionDescending => query.OrderByDescending(fund => fund.Description).ThenBy(fund => fund.Name).ThenBy(fund => fund.Id),
            _ => query.OrderBy(fund => fund.Name).ThenBy(fund => fund.Id),
        };
        int totalCount = await query.CountAsync(cancellationToken);
        List<FundModel> items = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue)
            .Select(fund => new FundModel { Id = fund.Id.Value, Name = fund.Name, Description = fund.Description })
            .ToListAsync(cancellationToken);
        return new CollectionModel<FundModel> { Items = items, TotalCount = totalCount };
    }

    /// <summary>
    /// Retrieves a Fund by ID.
    /// </summary>
    public Task<FundModel?> GetByIdAsync(Guid fundId, CancellationToken cancellationToken = default) =>
        databaseContext.Funds.AsNoTracking().Where(fund => fund.Id == new FundId(fundId))
            .Select(fund => new FundModel { Id = fund.Id.Value, Name = fund.Name, Description = fund.Description })
            .SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Retrieves Funds with their current balances.
    /// </summary>
    public async Task<CollectionModel<FundWithBalanceModel>> GetWithBalancesAsync(FundWithBalanceQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        IQueryable<Fund> funds = ApplyFilter(databaseContext.Funds.AsNoTracking(), request.Filter);
        IQueryable<FundWithBalanceModel> query = funds.Select(fund => new FundWithBalanceModel
        {
            Id = fund.Id.Value,
            Name = fund.Name,
            Description = fund.Description,
            CurrentBalance = databaseContext.FundBalanceHistories.Where(history => history.FundId == fund.Id)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                .Select(history => new FundBalanceModel { PostedBalance = history.PostedBalance, PendingDebitAmount = history.PendingDebitAmount, PendingCreditAmount = history.PendingCreditAmount })
                .FirstOrDefault() ?? new FundBalanceModel { PostedBalance = fund.OnboardedBalance ?? 0, PendingDebitAmount = 0, PendingCreditAmount = 0 },
        });
        query = request.Sort switch
        {
            FundWithBalanceSortModel.Name => query.OrderBy(fund => fund.Name),
            FundWithBalanceSortModel.NameDescending => query.OrderByDescending(fund => fund.Name),
            FundWithBalanceSortModel.Description => query.OrderBy(fund => fund.Description).ThenBy(fund => fund.Name),
            FundWithBalanceSortModel.DescriptionDescending => query.OrderByDescending(fund => fund.Description).ThenBy(fund => fund.Name),
            FundWithBalanceSortModel.PostedBalance => query.OrderBy(fund => fund.CurrentBalance.PostedBalance).ThenBy(fund => fund.Name),
            FundWithBalanceSortModel.PostedBalanceDescending => query.OrderByDescending(fund => fund.CurrentBalance.PostedBalance).ThenBy(fund => fund.Name),
            _ => query.OrderBy(fund => fund.Name),
        };
        int totalCount = await query.CountAsync(cancellationToken);
        List<FundWithBalanceModel> items = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new CollectionModel<FundWithBalanceModel> { Items = items, TotalCount = totalCount };
    }

    /// <summary>
    /// Applies the filter to the provided query
    /// </summary>
    private static IQueryable<Fund> ApplyFilter(IQueryable<Fund> query, FundFilterModel? filter)
    {
        if (!string.IsNullOrWhiteSpace(filter?.NameSearch))
        {
            query = query.Where(fund => fund.Name.Contains(filter.NameSearch));
        }
        if (filter?.Names is { Count: > 0 } names)
        {
            query = query.Where(fund => names.Contains(fund.Name));
        }
        return query;
    }
}