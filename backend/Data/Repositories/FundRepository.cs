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
    public bool DoesFundWithIdExist(Guid id) => databaseContext.Funds.Any(fund => fund.Id.Value == id);

    /// <inheritdoc/>
    public Fund FindById(FundId id) => databaseContext.Funds.Single(fund => fund.Id == id);

    /// <inheritdoc/>
    public Fund? FindByNameOrNull(string name) => databaseContext.Funds.FirstOrDefault(fund => fund.Name == name);

    /// <inheritdoc/>
    public void Add(Fund fund) => databaseContext.Add(fund);
}