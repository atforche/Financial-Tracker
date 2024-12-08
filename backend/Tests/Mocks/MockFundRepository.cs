using Domain.Aggregates.Funds;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Funds for testing
/// </summary>
public class MockFundRepository : IFundRepository
{
    private readonly List<Fund> _funds;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockFundRepository()
    {
        _funds = [];
    }

    /// <inheritdoc/>
    public Fund? FindByExternalIdOrNull(Guid id) => _funds.SingleOrDefault(fund => fund.Id.ExternalId == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => _funds;

    /// <inheritdoc/>
    public Fund? FindByNameOrNull(string name) => _funds.SingleOrDefault(fund => fund.Name == name);

    /// <inheritdoc/>
    public void Add(Fund fund) => _funds.Add(fund);
}