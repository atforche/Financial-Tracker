using System.Diagnostics.CodeAnalysis;
using Domain.Funds;

namespace Data.Funds;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository(DatabaseContext databaseContext) : IFundRepository
{
    #region IFundRepository

    /// <inheritdoc/>
    public Fund GetById(FundId id) => databaseContext.Funds.Single(fund => fund.Id == id);

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

    #endregion

    /// <summary>
    /// Gets the Funds that match the specified criteria
    /// </summary>
    public PaginatedCollection<Fund> GetMany(GetFundsRequest request)
    {
        List<FundSortModel> filteredFunds = new FundFilterer(databaseContext).Get(request);
        filteredFunds.Sort(new FundComparer(request.SortBy));
        return new PaginatedCollection<Fund>
        {
            Items = GetPagedFunds(filteredFunds.Select(f => f.Fund).ToList(), request.Limit, request.Offset),
            TotalCount = filteredFunds.Count,
        };
    }

    /// <summary>
    /// Attempts to get the Fund with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Fund? fund)
    {
        fund = databaseContext.Funds.FirstOrDefault(fund => ((Guid)(object)fund.Id) == id);
        return fund != null;
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