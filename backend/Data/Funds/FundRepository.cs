using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;

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
        string query = """
                        select Funds.* from Funds 
                        left join FundBalanceHistories on Funds.Id = FundBalanceHistories.FundId 
                            and FundBalanceHistories.Date = (select max(Date) from FundBalanceHistories where FundBalanceHistories.FundId = Funds.Id)
                            and FundBalanceHistories.Sequence = (select max(Sequence) from FundBalanceHistories where FundBalanceHistories.FundId = Funds.Id and FundBalanceHistories.Date = (select max(Date) from FundBalanceHistories where FundBalanceHistories.FundId = Funds.Id))
                        """;
        if (request.Search != null)
        {
            query += $" where Funds.Name like '%{request.Search}%' or Funds.Description like '%{request.Search}%' or FundBalanceHistories.PostedBalance like '%{request.Search}%'";
        }
        if (request.Sort is null or FundSortOrder.Name)
        {
            query += $" order by Funds.Name asc";
        }
        else if (request.Sort == FundSortOrder.NameDescending)
        {
            query += $" order by Funds.Name desc";
        }
        else if (request.Sort == FundSortOrder.Description)
        {
            query += $" order by Funds.Description asc, Funds.Name asc";
        }
        else if (request.Sort == FundSortOrder.DescriptionDescending)
        {
            query += $" order by Funds.Description desc, Funds.Name asc";
        }
        else if (request.Sort == FundSortOrder.Balance)
        {
            query += $" order by FundBalanceHistories.PostedBalance asc, Funds.Name asc";
        }
        else if (request.Sort == FundSortOrder.BalanceDescending)
        {
            query += $" order by FundBalanceHistories.PostedBalance desc, Funds.Name asc";
        }

        var funds = databaseContext.Funds.FromSqlRaw(query).ToList();
        return new PaginatedCollection<Fund>
        {
            Items = GetPagedFunds(funds, request.Limit, request.Offset),
            TotalCount = funds.Count,
        };
    }

    /// <summary>
    /// Gets the Funds within the specified Accounting Period that match the specified criteria
    /// </summary>
    public PaginatedCollection<Fund> GetManyByAccountingPeriod(AccountingPeriodId _, GetAccountingPeriodFundsRequest request)
    {
        string query = "select * from Funds";
        if (request.Search != null)
        {
            query += $" where Name like '%{request.Search}%' or Description like '%{request.Search}%'";
        }
        if (request.Sort is null or AccountingPeriodFundSortOrder.Name)
        {
            query += $" order by Name asc";
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.NameDescending)
        {
            query += $" order by Name desc";
        }

        var funds = databaseContext.Funds.FromSqlRaw(query).ToList();
        return new PaginatedCollection<Fund>
        {
            Items = GetPagedFunds(funds, request.Limit, request.Offset),
            TotalCount = funds.Count,
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