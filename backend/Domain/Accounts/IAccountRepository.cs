using System.Diagnostics.CodeAnalysis;

namespace Domain.Accounts;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Account"/>
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Gets all the Accounts currently in the repository
    /// </summary>
    IReadOnlyCollection<Account> GetAll(GetAllAccountsRequest request);

    /// <summary>
    /// Gets the Account with the specified ID.
    /// </summary>
    Account GetById(AccountId id);

    /// <summary>
    /// Attempts to get the Account with the specified ID
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out Account? account);

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

/// <summary>
/// Request to retrieve all the Accounts that match the specified criteria
/// </summary>
public record GetAllAccountsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountSortOrder? SortBy { get; init; }

    /// <summary>
    /// Account names to include in the results
    /// </summary>
    public IReadOnlyCollection<string>? Names { get; init; }

    /// <summary>
    /// Account types to include in the results
    /// </summary>
    public IReadOnlyCollection<AccountType>? Types { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}