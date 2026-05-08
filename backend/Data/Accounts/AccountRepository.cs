using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Data.Accounts;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository(DatabaseContext databaseContext) : IAccountRepository
{
    #region IAccountRepository

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> GetAll() => databaseContext.Accounts.ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> GetAllAccountsAddedInPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Accounts.Where(account => account.AddAccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public Account GetById(AccountId id) => databaseContext.Accounts.SingleOrDefault(account => account.Id == id)
        ?? databaseContext.Accounts.Local.Single(account => account.Id == id);

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

    #endregion

    /// <summary>
    /// Attempts to get the Account with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => ((Guid)(object)account.Id) == id);
        return account != null;
    }
}