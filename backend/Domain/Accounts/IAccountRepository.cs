using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;

namespace Domain.Accounts;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Account"/>
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Gets all Accounts in the repository.
    /// </summary>
    IReadOnlyCollection<Account> GetAll();

    /// <summary>
    /// Gets all the Accounts that were added in the specified accounting period.
    /// </summary>
    IReadOnlyCollection<Account> GetAllAccountsAddedInPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Gets the Account with the specified ID.
    /// </summary>
    Account GetById(AccountId id);

    /// <summary>
    /// Attempts to get the Account with the specified name
    /// </summary>
    bool TryGetByName(string name, [NotNullWhen(true)] out Account? account);

    /// <summary>
    /// Adds the provided Account to the repository
    /// </summary>
    void Add(Account account);

    /// <summary>
    /// Deletes the provided Account from the repository
    /// </summary>
    void Delete(Account account);
}