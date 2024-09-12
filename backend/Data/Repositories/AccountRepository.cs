using Data.Models;
using Domain.Entities;
using Domain.Events;
using Domain.Factories;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
/// Account repository that allows accounts to be persisted to the database
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly DatabaseContext _context;
    private readonly IAccountFactory _accountFactory;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    /// <param name="accountFactory">Factory used to construct Account instances</param>
    public AccountRepository(DatabaseContext context, IAccountFactory accountFactory)
    {
        _context = context;
        _accountFactory = accountFactory;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> FindAll() => _context.Accounts.Select(ConvertToEntity).ToList();

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
    public void Add(Account account)
    {
        var accountData = PopulateFromAccount(account, null);
        _context.Add(accountData);
    }

    /// <inheritdoc/>
    public void Update(Account account)
    {
        AccountData accountData = _context.Accounts.Single(accountData => accountData.Id == account.Id);
        PopulateFromAccount(account, accountData);
    }

    /// <inheritdoc/>
    public void Delete(Guid id)
    {
        AccountData accountData = _context.Accounts.Single(accountData => accountData.Id == id);
        _context.Accounts.Remove(accountData);
    }

    /// <summary>
    /// Converts the provided <see cref="AccountData"/> object into an <see cref="Account"/> domain entity.
    /// </summary>
    private Account ConvertToEntity(AccountData accountData) => _accountFactory.Recreate(
        new AccountRecreateRequest(accountData.Id, accountData.Name, accountData.Type, accountData.IsActive));

    /// <summary>
    /// Converts the provided <see cref="Account"/> entity into an <see cref="AccountData"/> data object
    /// </summary>
    private static AccountData PopulateFromAccount(Account account, AccountData? existingAccountData)
    {
        AccountData newAccountData = new AccountData()
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            IsActive = account.IsActive,
        };
        existingAccountData?.Replace(newAccountData);

        AccountData accountData = existingAccountData ?? newAccountData;
        foreach (IDomainEvent domainEvent in account.GetDomainEvents())
        {
            accountData.RaiseEvent(domainEvent);
        }
        return accountData;
    }

    private sealed record AccountRecreateRequest(Guid Id, string Name, AccountType Type, bool IsActive) : IRecreateAccountRequest;
}