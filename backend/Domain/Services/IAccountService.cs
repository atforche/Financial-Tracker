using Domain.Entities;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to create or modify Accounts
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="createAccountRequest">Request to create an Account</param>
    /// <param name="startingBalance">Starting balance for this Account</param>
    /// <param name="newAccount">The newly created Account</param>
    /// <param name="newAccountStartingBalance">The Account Starting Balance for the newly created Account</param>
    void CreateNewAccount(CreateAccountRequest createAccountRequest,
        decimal startingBalance,
        out Account newAccount,
        out AccountStartingBalance newAccountStartingBalance);
}