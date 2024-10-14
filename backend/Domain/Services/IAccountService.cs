using Domain.Entities;
using Domain.Factories;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to create or modify Accounts
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates a new Account with the provided properties and starting balance
    /// </summary>
    /// <param name="createAccountRequest">Request to create an Account</param>
    /// <param name="newAccount">The newly created Account</param>
    /// <param name="newAccountStartingBalance">The starting balance for the newly created Account</param>
    void CreateNewAccount(CreateAccountRequest createAccountRequest,
        out Account newAccount,
        out AccountStartingBalance newAccountStartingBalance);

    /// <summary>
    /// Renames the provided Account to the new name
    /// </summary>
    /// <param name="account">Account to be renamed</param>
    /// <param name="newName">New name to give to the Account</param>
    void RenameAccount(Account account, string newName);
}

/// <summary>
/// Record representing a request to create an Account
/// </summary>
public record CreateAccountRequest
{
    /// <see cref="Account.Name"/>
    public required string Name { get; init; }

    /// <see cref="Account.Type"/>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Starting balance for this Account
    /// </summary>
    public required CreateAccountStartingBalanceRequest StartingBalance { get; init; }
}