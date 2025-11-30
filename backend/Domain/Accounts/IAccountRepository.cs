using System.Diagnostics.CodeAnalysis;

namespace Domain.Accounts;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Account"/>
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Finds all the Accounts currently in the repository
    /// </summary>
    /// <returns>All the Accounts in the repository</returns>
    IReadOnlyCollection<Account> FindAll();

    /// <summary>
    /// Finds the Account with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Account to find</param>
    /// <returns>The Account that was found</returns>
    Account FindById(AccountId id);

    /// <summary>
    /// Attempts to find the Account with the specified ID
    /// </summary>
    /// <param name="id">ID of the Account to find</param>
    /// <param name="account">The Account that was found, or null if one wasn't found</param>
    /// <returns>True if an Account with the provided ID was found, false otherwise</returns>
    bool TryFindById(Guid id, [NotNullWhen(true)] out Account? account);

    /// <summary>
    /// Attempts to find the Account with the specified name
    /// </summary>
    /// <param name="name">Name of the Account to find</param>
    /// <param name="account">The Account that was found, or null if one wasn't found</param>
    /// <returns>True if an Account with the provided name was found, false otherwise</returns>
    bool TryFindByName(string name, [NotNullWhen(true)] out Account? account);

    /// <summary>
    /// Adds the provided Account to the repository
    /// </summary>
    /// <param name="account">Account that should be added</param>
    void Add(Account account);

    /// <summary>
    /// Deletes the provided Account from the repository
    /// </summary>
    /// <param name="account">Account to be deleted</param>
    void Delete(Account account);
}