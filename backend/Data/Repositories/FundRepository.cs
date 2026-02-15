using System.Diagnostics.CodeAnalysis;
using Domain.Funds;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository(DatabaseContext databaseContext) : IFundRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> GetAll(GetAllFundsRequest request)
    {
        IQueryable<Fund> filteredFunds = GetFilteredFunds(request);
        List<Fund> sortedFunds = GetSortedFunds(filteredFunds, request.SortBy);
        return GetPagedFunds(sortedFunds, request.Limit, request.Offset);
    }

    /// <inheritdoc/>
    public Fund GetById(FundId id) => databaseContext.Funds.Single(fund => fund.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Fund? fund)
    {
        fund = databaseContext.Funds.FirstOrDefault(fund => ((Guid)(object)fund.Id) == id);
        return fund != null;
    }

    /// <inheritdoc/>
    public bool TryGetByName(string name, [NotNullWhen(true)] out Fund? fund)
    {
        fund = databaseContext.Funds.FirstOrDefault(f => f.Name == name);
        return fund != null;
    }

    /// <inheritdoc/>
    public void Add(Fund fund) => databaseContext.Add(fund);

    /// <inheritdoc/>
    public void Delete(Fund fund) => databaseContext.Remove(fund);

    /// <summary>
    /// Gets the filtered collection of Funds based on the provided request
    /// </summary>
    private IQueryable<Fund> GetFilteredFunds(GetAllFundsRequest request)
    {
        IQueryable<Fund> results = databaseContext.Funds.AsQueryable();
        if (request.Names != null && request.Names.Count > 0)
        {
            results = results.Where(fund => request.Names.Contains(fund.Name));
        }
        return results;
    }

    /// <summary>
    /// Gets the sorted collection of Funds based on the provided request
    /// </summary>
    private List<Fund> GetSortedFunds(IQueryable<Fund> filteredFunds, FundSortOrder? sortBy)
    {
        if (sortBy is null or FundSortOrder.Name)
        {
            return filteredFunds.OrderBy(fund => fund.Name).ToList();
        }
        if (sortBy == FundSortOrder.NameDescending)
        {
            return filteredFunds.OrderByDescending(fund => fund.Name).ToList();
        }
        if (sortBy == FundSortOrder.Description)
        {
            return filteredFunds.OrderBy(fund => fund.Description).ThenBy(fund => fund.Name).ToList();
        }
        if (sortBy == FundSortOrder.DescriptionDescending)
        {
            return filteredFunds.OrderByDescending(fund => fund.Description).ThenBy(fund => fund.Name).ToList();
        }
        var fundsWithBalance = filteredFunds.GroupJoin(databaseContext.FundBalanceHistories,
            fund => fund.Id,
            history => history.FundId,
            (fund, histories) => new { fund, currentBalance = histories.OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).FirstOrDefault() }).ToList();
        return sortBy == FundSortOrder.Balance
            ? fundsWithBalance.OrderBy(item => item.currentBalance?.AccountBalances.Sum(accountBalance => accountBalance.Amount) ?? 0).ThenBy(item => item.fund.Name).Select(item => item.fund).ToList()
            : fundsWithBalance.OrderByDescending(item => item.currentBalance?.AccountBalances.Sum(accountBalance => accountBalance.Amount) ?? 0).ThenBy(item => item.fund.Name).Select(item => item.fund).ToList();
    }

    /// <summary>
    /// Gets the paged collection of Funds based on the provided request
    /// </summary>
    private static List<Fund> GetPagedFunds(List<Fund> sortedFunds, int? limit, int? offset)
    {
        if (offset != null)
        {
            sortedFunds = sortedFunds.Skip(offset.Value).ToList();
        }
        if (limit != null)
        {
            sortedFunds = sortedFunds.Take(limit.Value).ToList();
        }
        return sortedFunds;
    }
}