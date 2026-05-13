using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Funds;

namespace Data.Funds;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository(DatabaseContext databaseContext) : IFundRepository
{
    #region IFundRepository

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> GetAll() => databaseContext.Funds.ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> GetAllFundsAddedInPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Funds.Where(fund => fund.OpeningAccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public Fund GetById(FundId id) => databaseContext.Funds.Single(fund => fund.Id == id);

    /// <inheritdoc/>
    public Fund? GetSystemFund() => databaseContext.Funds.FirstOrDefault(fund => fund.IsSystemFund);

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
    /// Attempts to get the Fund with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Fund? fund)
    {
        fund = databaseContext.Funds.FirstOrDefault(fund => ((Guid)(object)fund.Id) == id);
        return fund != null;
    }
}