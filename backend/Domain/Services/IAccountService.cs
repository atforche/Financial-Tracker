using Domain.Entities;
using Domain.ValueObjects;

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
    /// <param name="newAccount">The newly created Account</param>
    /// <param name="newAccountStartingBalance">The Account Starting Balance for the newly created Account</param>
    void CreateNewAccount(CreateAccountRequest createAccountRequest,
        out Account newAccount,
        out AccountStartingBalance newAccountStartingBalance);
}

/// <summary>
/// Record representing a request to create an Account
/// </summary>
public record CreateAccountRequest
{
    /// <inheritdoc cref="Account.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="Account.Type"/>
    public required AccountType Type { get; init; }

    /// <inheritdoc cref="AccountStartingBalance.StartingFundBalances"/>
    public required IEnumerable<CreateFundAmountRequest> StartingFundBalances { get; init; }
}