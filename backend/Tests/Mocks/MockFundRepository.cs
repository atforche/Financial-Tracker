using System.Diagnostics.CodeAnalysis;
using Domain.Funds;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Funds for testing
/// </summary>
internal sealed class MockFundRepository : IFundRepository
{
    private readonly Dictionary<Guid, Fund> _funds;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockFundRepository() => _funds = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => _funds.Values;

    /// <inheritdoc/>
    public Fund FindById(FundId id) => _funds[id.Value];

    /// <inheritdoc/>
    public bool TryFindById(Guid id, [NotNullWhen(true)] out Fund? fund) => _funds.TryGetValue(id, out fund);

    /// <inheritdoc/>
    public bool TryFindByName(string name, [NotNullWhen(true)] out Fund? fund)
    {
        fund = _funds.Values.FirstOrDefault(f => f.Name == name);
        return fund != null;
    }

    /// <inheritdoc/>
    public void Add(Fund fund) => _funds.Add(fund.Id.Value, fund);

    /// <inheritdoc/>
    public void Delete(Fund fund) => _funds.Remove(fund.Id.Value);
}