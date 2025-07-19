using Domain.Funds;

namespace Tests.Old.Mocks;

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
    public bool DoesFundWithIdExist(Guid id) => _funds.ContainsKey(id);

    /// <inheritdoc/>
    public Fund FindById(FundId id) => _funds[id.Value];

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => _funds.Values;

    /// <inheritdoc/>
    public Fund? FindByNameOrNull(string name) => _funds.Values.SingleOrDefault(fund => fund.Name == name);

    /// <inheritdoc/>
    public void Add(Fund fund) => _funds.Add(fund.Id.Value, fund);
}