using Data.Models;
using Domain.Entities;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
/// Account repository that allows accounts to be persisted to the database
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly DatabaseContext _context;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public AccountRepository(DatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() =>
        _context.Accounts.Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public Account Find(Guid id) => FindOrNull(id) ?? throw new KeyNotFoundException();

    /// <inheritdoc/>
    public Account? FindOrNull(Guid id)
    {
        AccountData? accountData = _context.Accounts.FirstOrDefault(account => account.Id == id);
        return accountData != null ? ConvertToEntity(accountData) : null;
    }

    /// <inheritdoc/>
    public Account? FindByNameOrNull(string name)
    {
        AccountData? accountData = _context.Accounts.FirstOrDefault(account => account.Name == name);
        return accountData != null ? ConvertToEntity(accountData) : null;
    }

    /// <inheritdoc/>
    public void Commit(Account account)
    {
        _context.Add(ConvertToData(account));
        _context.SaveChanges();
    }

    /// <inheritdoc/>
    public void Update(Account account)
    {
        AccountData accountData = _context.Accounts.Single(accountData => accountData.Id == account.Id);
        if (account.Name != accountData.Name)
        {
            accountData.Name = account.Name;
        }
        _context.SaveChanges();
    }

    /// <inheritdoc/>
    public void Delete(Guid id)
    {
        AccountData accountData = _context.Accounts.Single(accountData => accountData.Id == id);
        _context.Accounts.Remove(accountData);
        _context.SaveChanges();
    }

    /// <summary>
    /// Converts the provided <see cref="AccountData"/> object into an <see cref="Account"/> domain entity.
    /// </summary>
    private static Account ConvertToEntity(AccountData accountData) =>
        new(accountData.Id, accountData.Name, accountData.Type, accountData.IsActive);

    /// <summary>
    /// Converts the provided <see cref="Account"/> entity into an <see cref="AccountData"/> data object
    /// </summary>
    private static AccountData ConvertToData(Account account) => new AccountData()
    {
        Id = account.Id,
        Name = account.Name,
        Type = account.Type,
        IsActive = account.IsActive,
    };
}