using Domain.Accounts;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Accounts for testing
/// </summary>
internal sealed class MockAccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _accounts;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockAccountRepository() => _accounts = [];

    /// <inheritdoc/>
    public bool DoesAccountWithIdExist(Guid id) => _accounts.ContainsKey(id);

    /// <inheritdoc/>
    public Account FindById(AccountId id) => _accounts[id.Value];

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() => _accounts.Values;

    /// <inheritdoc/>
    public Account? FindByNameOrNull(string name) => _accounts.Values.SingleOrDefault(account => account.Name == name);

    /// <inheritdoc/>
    public void Add(Account account) => _accounts.Add(account.Id.Value, account);
}