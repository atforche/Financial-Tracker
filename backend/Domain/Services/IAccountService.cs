using Domain.Aggregates.Accounts;
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
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    /// <param name="startingFundBalances">Starting Fund Balances for this Account</param>
    /// <returns>The newly created Account</returns>
    Account CreateNewAccount(string name, AccountType type, IEnumerable<FundAmount> startingFundBalances);
}