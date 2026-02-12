using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository(DatabaseContext databaseContext) : IAccountRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Account> GetAll() => databaseContext.Accounts.OrderBy(account => account.Name).ToList();

    /// <inheritdoc/>
    public Account GetById(AccountId id) => databaseContext.Accounts.Single(account => account.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => ((Guid)(object)account.Id) == id);
        return account != null;
    }

    /// <inheritdoc/>
    public bool TryGetByName(string name, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => account.Name == name);
        return account != null;
    }

    /// <inheritdoc/>
    public void Add(Account account) => databaseContext.Add(account);

    /// <inheritdoc/>
    public void Delete(Account account) => databaseContext.Remove(account);
}