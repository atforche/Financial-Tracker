using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository(DatabaseContext databaseContext) : IAccountRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() => databaseContext.Accounts.ToList();

    /// <inheritdoc/>
    public Account FindById(AccountId id) => databaseContext.Accounts.Single(account => account.Id == id);

    /// <inheritdoc/>
    public bool TryFindById(Guid id, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => ((Guid)(object)account.Id) == id);
        return account != null;
    }

    /// <inheritdoc/>
    public bool TryFindByName(string name, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => account.Name == name);
        return account != null;
    }

    /// <inheritdoc/>
    public void Add(Account account) => databaseContext.Add(account);

    /// <inheritdoc/>
    public void Delete(Account account) => databaseContext.Remove(account);
}