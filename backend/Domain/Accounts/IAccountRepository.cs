namespace Domain.Accounts;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Account"/>
/// </summary>
public interface IAccountRepository : IAggregateRepository<Account>
{
    /// <summary>
    /// Finds all the Accounts currently in the repository
    /// </summary>
    /// <returns>All the Accounts in the repository</returns>
    IReadOnlyCollection<Account> FindAll();

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