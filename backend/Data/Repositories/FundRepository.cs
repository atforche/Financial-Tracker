using System.Diagnostics.CodeAnalysis;
using Domain.Funds;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository(DatabaseContext databaseContext) : IFundRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> GetAll(FundSortOrder? sortBy = null)
    {
        if (sortBy is null or FundSortOrder.Name)
        {
            return databaseContext.Funds.OrderBy(fund => fund.Name).ToList();
        }
        if (sortBy == FundSortOrder.NameDescending)
        {
            return databaseContext.Funds.OrderByDescending(fund => fund.Name).ToList();
        }
        if (sortBy == FundSortOrder.Description)
        {
            return databaseContext.Funds.OrderBy(fund => fund.Description).ThenBy(fund => fund.Name).ToList();
        }
        if (sortBy == FundSortOrder.DescriptionDescending)
        {
            return databaseContext.Funds.OrderByDescending(fund => fund.Description).ThenBy(fund => fund.Name).ToList();
        }
        var fundsWithBalance = databaseContext.Funds.GroupJoin(databaseContext.FundBalanceHistories,
            fund => fund.Id,
            history => history.FundId,
            (fund, histories) => new { fund, currentBalance = histories.OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).FirstOrDefault() }).ToList();
        return sortBy == FundSortOrder.Balance
            ? fundsWithBalance.OrderBy(item => item.currentBalance?.AccountBalances.Sum(accountBalance => accountBalance.Amount) ?? 0).ThenBy(item => item.fund.Name).Select(item => item.fund).ToList()
            : fundsWithBalance.OrderByDescending(item => item.currentBalance?.AccountBalances.Sum(accountBalance => accountBalance.Amount) ?? 0).ThenBy(item => item.fund.Name).Select(item => item.fund).ToList();
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
}