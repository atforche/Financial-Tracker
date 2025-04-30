using Domain.Aggregates.Funds;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository(DatabaseContext context) : AggregateRepository<Fund>(context), IFundRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => DatabaseContext.Funds.ToList();

    /// <inheritdoc/>
    public Fund? FindByNameOrNull(string name) => DatabaseContext.Funds.FirstOrDefault(fund => fund.Name == name);

    /// <inheritdoc/>
    public void Add(Fund fund) => DatabaseContext.Add(fund);
}