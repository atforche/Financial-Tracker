using Data.EntityModels;
using Domain.Entities;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
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
    public IReadOnlyCollection<Account> FindAll() => _context.Accounts.Select(ConvertToEntity).ToList();

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

    /// <summary>
    /// Converts the provided <see cref="AccountData"/> object into an <see cref="Account"/> domain entity.
    /// </summary>
    /// <param name="accountData">Account Data to be converted</param>
    /// <returns>The converted Account domain entity</returns>
    private Account ConvertToEntity(AccountData accountData) => new Account(
        new AccountRecreateRequest
        {
            Id = accountData.Id,
            Name = accountData.Name,
            Type = accountData.Type,
        });

    /// <summary>
    /// Converts the provided <see cref="Account"/> entity into an <see cref="AccountData"/> data object
    /// </summary>
    /// <param name="account">Account entity to convert</param>
    /// <param name="existingAccountData">Existing Account Data model to populate from the entity, or null if a new model should be created</param>
    /// <returns>The converted Account Data</returns>
    private static AccountData PopulateFromAccount(Account account, AccountData? existingAccountData)
    {
        AccountData newAccountData = new AccountData()
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
        };
        existingAccountData?.Replace(newAccountData);
        return existingAccountData ?? newAccountData;
    }

    /// <inheritdoc/>
    private sealed record AccountRecreateRequest : IRecreateAccountRequest
    {
        /// <inheritdoc/>
        public required Guid Id { get; init; }

        /// <inheritdoc/>
        public required string Name { get; init; }

        /// <inheritdoc/>
        public required AccountType Type { get; init; }
    };
}