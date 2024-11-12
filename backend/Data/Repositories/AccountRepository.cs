using Domain.Aggregates.Accounts;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository : AggregateRepositoryBase<Account>, IAccountRepository
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public AccountRepository(DatabaseContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() => DatabaseContext.Accounts.ToList();

    /// <inheritdoc/>
    public Account? FindByNameOrNull(string name) => DatabaseContext.Accounts.FirstOrDefault(account => account.Name == name);

    /// <inheritdoc/>
    public void Add(Account account) => DatabaseContext.Add(account);
}