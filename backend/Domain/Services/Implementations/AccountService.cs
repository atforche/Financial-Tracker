using Domain.Entities;
using Domain.Factories;
using Domain.Repositories;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountStartingBalanceFactory _accountStartingBalanceFactory;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountStartingBalanceFactory">Factory that constructs instances of Account Starting Balance</param>
    public AccountService(
        IAccountRepository accountRepository,
        IAccountStartingBalanceFactory accountStartingBalanceFactory)
    {
        _accountRepository = accountRepository;
        _accountStartingBalanceFactory = accountStartingBalanceFactory;
    }

    /// <inheritdoc/>
    public void CreateNewAccount(CreateAccountRequest createAccountRequest,
        out Account newAccount,
        out AccountStartingBalance newAccountStartingBalance)
    {
        ValidateNewAccountName(createAccountRequest.Name);
        newAccount = new Account(Guid.NewGuid(), createAccountRequest.Name, createAccountRequest.Type, true);
        newAccount.Validate();

        newAccountStartingBalance = _accountStartingBalanceFactory.Create(newAccount, createAccountRequest.StartingBalance);
        newAccountStartingBalance.Validate();
    }

    /// <inheritdoc/>
    public void RenameAccount(Account account, string newName)
    {
        ValidateNewAccountName(newName);
        account.Name = newName;
    }

    /// <summary>
    /// Validates that the new name for an account is unique among the existing accounts
    /// </summary>
    /// <param name="newName">New name to be given to an Account</param>
    private void ValidateNewAccountName(string newName)
    {
        if (_accountRepository.FindByNameOrNull(newName) != null)
        {
            throw new InvalidOperationException();
        }
    }
}