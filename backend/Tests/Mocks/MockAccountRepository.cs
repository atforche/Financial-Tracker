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
    public bool DoesAccountWithIdExist(Guid id) => _accounts.Any(account => account.Id.Value == id);

    /// <inheritdoc/>
    public Account FindById(AccountId id) => _accounts.Single(account => account.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() => _accounts;

    /// <inheritdoc/>
    public Account? FindByNameOrNull(string name) => _accounts.SingleOrDefault(account => account.Name == name);

    /// <inheritdoc/>
    public void Add(Account account) => _accounts.Add(account);
}