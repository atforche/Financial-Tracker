using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

/// <summary>
/// Application service that exposes basic functionality related to Accounts
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates a new account with the provided name and returns the account.
    /// </summary>
    /// <param name="name">Name of the new account to be created</param>
    /// <param name="type">Type of the new account to be created</param>
    /// <param name="isActive">Is active flag of the new account to be created</param>
    /// <returns>The newly created account</returns>
    public Account CreateAccount(string name, AccountType type, bool isActive);
}

/// <summary>
/// Application service that exposes basic functionality related to Accounts
/// </summary>
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="repository">Repository of accounts</param>
    public AccountService(IAccountRepository repository)
    {
        _accountRepository = repository;
    }

    /// <inheritdoc/>
    public Account CreateAccount(string name, AccountType type, bool isActive)
    {
        Account? accountWithSameName = _accountRepository.FindByNameOrNull(name);
        if (accountWithSameName != null)
        {
            throw new InvalidOperationException();
        }
        var account = new Account(name, type, isActive);
        _accountRepository.Commit(account);
        return account;
    }
}