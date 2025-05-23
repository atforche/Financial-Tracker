using Domain.Funds;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Funds for testing
/// </summary>
internal sealed class MockFundRepository : IFundRepository
{
    private readonly List<Fund> _funds;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockFundRepository() => _funds = [];

    /// <inheritdoc/>
    public Fund? FindByIdOrNull(FundId id) => _funds.SingleOrDefault(fund => fund.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => _funds;

    /// <inheritdoc/>
    public Fund? FindByNameOrNull(string name) => _funds.SingleOrDefault(fund => fund.Name == name);

    /// <inheritdoc/>
    public void Add(Fund fund) => _funds.Add(fund);
}