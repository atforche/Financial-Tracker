using Domain.Entities;

namespace Domain.Factories;

/// <summary>
/// Interface representing a factory responsible for constructing instances of an Account
/// </summary>
public interface IAccountFactory
{
    /// <summary>
    /// Recreates an existing Account with the provided properties
    /// </summary>
    /// <param name="request">Request to recreate an Account</param>
    /// <returns>The recreated Account</returns>
    Account Recreate(IRecreateAccountRequest request);
}

/// <summary>
/// Interface representing a request to recreate an existing Account
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