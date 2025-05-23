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
    public bool DoesAccountWithIdExist(Guid id) => databaseContext.Accounts.Any(account => account.Id.Value == id);

    /// <inheritdoc/>
    public Account FindById(AccountId id) => databaseContext.Accounts.Single(account => account.Id == id);

    /// <inheritdoc/>
    public Account? FindByNameOrNull(string name) => databaseContext.Accounts.FirstOrDefault(account => account.Name == name);

    /// <inheritdoc/>
    public void Add(Account account) => databaseContext.Add(account);
}