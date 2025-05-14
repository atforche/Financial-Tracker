using Domain.Accounts;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Accounts for testing
/// </summary>
internal sealed class MockAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockAccountRepository() => _accounts = [];

    /// <inheritdoc/>
    public Account? FindByExternalIdOrNull(Guid id) => _accounts.SingleOrDefault(account => account.Id.ExternalId == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() => _accounts;

    /// <inheritdoc/>
    public Account? FindByNameOrNull(string name) => _accounts.SingleOrDefault(account => account.Name == name);

    /// <inheritdoc/>
    public void Add(Account account) => _accounts.Add(account);
}