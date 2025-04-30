using Domain.Aggregates.Accounts;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository(DatabaseContext context) : AggregateRepository<Account>(context), IAccountRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() => DatabaseContext.Accounts.ToList();

    /// <inheritdoc/>
    public Account? FindByNameOrNull(string name) => DatabaseContext.Accounts.FirstOrDefault(account => account.Name == name);

    /// <inheritdoc/>
    public void Add(Account account) => DatabaseContext.Add(account);
}