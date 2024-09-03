using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Account"/>
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Finds all the accounts currently in the repository.
    /// </summary>
    /// <returns>All the Accounts in the repository</returns>
    IReadOnlyCollection<Account> FindAll();

    /// <summary>
    /// Finds the Account with the specified id. An error is thrown if no account is found.
    /// </summary>
    /// <param name="id">Id of the Account to find</param>
    /// <returns>The Account that was found</returns>
    Account Find(Guid id);

    /// <summary>
    /// Finds the Account with the specified id.
    /// </summary>
    /// <param name="id">Id of the Account to find</param>
    /// <returns>The Account that was found, or null if one wasn't found</returns>
    Account? FindOrNull(Guid id);

    /// <summary>
    /// Finds the Account with the specified name.
    /// </summary>
    /// <param name="name">Name of the Account to find</param>
    /// <returns>The Account that was found, or null if one wasn't found</returns>
    Account? FindByNameOrNull(string name);

    /// <summary>
    /// Commits the provided Account to the repository.
    /// </summary>
    /// <param name="account">Account object that should be committed</param>
    void Commit(Account account);

    /// <summary>
    /// Updates the provided Account in the repository.
    /// </summary>
    void Update(Account account);

    /// <summary>
    /// Deletes the Account with the specified id.
    /// </summary>
    /// <param name="id">Id of the Account to delete</param>
    void Delete(Guid id);
}