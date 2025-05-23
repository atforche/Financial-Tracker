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
    /// Determines if an Account with the provided ID exists
    /// </summary>
    /// <param name="id">ID of the Account</param>
    /// <returns>True if an Account with the provided ID exists, false otherwise</returns>
    bool DoesAccountWithIdExist(Guid id);

    /// <summary>
    /// Finds the Account with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Account to find</param>
    /// <returns>The Account that was found</returns>
    Account FindById(AccountId id);

    /// <summary>
    /// Finds the Account with the specified name
    /// </summary>
    /// <param name="name">Name of the Account to find</param>
    /// <returns>The Account that was found, or null if one wasn't found</returns>
    Account? FindByNameOrNull(string name);

    /// <summary>
    /// Adds the provided Account to the repository
    /// </summary>
    /// <param name="account">Account that should be added</param>
    void Add(Account account);
}