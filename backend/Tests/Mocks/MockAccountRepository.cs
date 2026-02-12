using System.Diagnostics.CodeAnalysis;
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
    public IReadOnlyCollection<Account> GetAll() => _accounts.Values;

    /// <inheritdoc/>
    public Account GetById(AccountId id) => _accounts[id.Value];

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Account? account)
    {
        account = _accounts.GetValueOrDefault(id);
        return account != null;
    }

    /// <inheritdoc/>
    public bool TryGetByName(string name, [NotNullWhen(true)] out Account? account)
    {
        account = _accounts.Values.SingleOrDefault(account => account.Name == name);
        return account != null;
    }

    /// <inheritdoc/>
    public void Add(Account account) => _accounts.Add(account.Id.Value, account);

    /// <inheritdoc/>
    public void Delete(Account account) => _accounts.Remove(account.Id.Value);
}