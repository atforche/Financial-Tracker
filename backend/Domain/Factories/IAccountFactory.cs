using Domain.Entities;

namespace Domain.Factories;

/// <summary>
/// Interface representing a factory responsible for constructing instances of an Account
/// </summary>
public interface IAccountFactory
{
    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="type">Type for the Account</param>
    /// <param name="isActive">Is active flag for the Account</param>
    /// <returns>The newly created Account</returns>
    Account CreateNewAccount(string name, AccountType type, bool isActive);

    /// <summary>
    /// Recreates an existing Account with the provided properties
    /// </summary>
    /// <param name="request">Request to recreate an Account</param>
    /// <returns>The recreated Account</returns>
    Account RecreateExistingAccount(IRecreateAccountRequest request);
}

/// <summary>
/// Interface representing a request to recreate an existing account
/// </summary>
public interface IRecreateAccountRequest
{
    /// <summary>
    /// Id for this Account
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Name for this Account
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    AccountType Type { get; }

    /// <summary>
    /// Is active flag for this account
    /// </summary>
    bool IsActive { get; }
}