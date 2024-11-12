using Domain.Aggregates.Funds;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository : AggregateRepositoryBase<Fund>, IFundRepository
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public FundRepository(DatabaseContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => DatabaseContext.Funds.ToList();

    /// <inheritdoc/>
    public Fund? FindByNameOrNull(string name) => DatabaseContext.Funds.FirstOrDefault(fund => fund.Name == name);

    /// <inheritdoc/>
    public void Add(Fund fund) => DatabaseContext.Add(fund);
}