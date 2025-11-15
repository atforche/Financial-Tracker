using System.Diagnostics.CodeAnalysis;
using Domain.Funds;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository(DatabaseContext databaseContext) : IFundRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => databaseContext.Funds.ToList();

    /// <inheritdoc/>
    public Fund FindById(FundId id) => databaseContext.Funds.Single(fund => fund.Id == id);

    /// <inheritdoc/>
    public bool TryFindById(Guid id, [NotNullWhen(true)] out Fund? fund)
    {
        fund = databaseContext.Funds.FirstOrDefault(f => ((Guid)(object)f.Id) == id);
        return fund != null;
    }

    /// <inheritdoc/>
    public bool TryFindByName(string name, [NotNullWhen(true)] out Fund? fund)
    {
        fund = databaseContext.Funds.FirstOrDefault(f => f.Name == name);
        return fund != null;
    }

    /// <inheritdoc/>
    public void Add(Fund fund) => databaseContext.Add(fund);

    /// <inheritdoc/>
    public void Delete(Fund fund) => databaseContext.Remove(fund);
}