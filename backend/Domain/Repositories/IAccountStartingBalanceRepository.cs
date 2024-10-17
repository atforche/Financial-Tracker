using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="AccountStartingBalance"/>
/// </summary>
public interface IAccountStartingBalanceRepository
{
    /// <summary>
    /// Finds the Account Starting Balance for the provided Account and Accounting Period
    /// </summary>
    /// <param name="accountId">ID of the Account for the Account Starting Balance to find</param>
    /// <param name="accountingPeriodId">ID of the Accounting Period for the Account Starting Balance to find</param>
    /// <returns>The Account Starting Balance that was found, or null if one wasn't found</returns>
    AccountStartingBalance? FindOrNull(Guid accountId, Guid accountingPeriodId);

    /// <summary>
    /// Adds the provided Account Starting Balance to the repository
    /// </summary>
    /// <param name="accountStartingBalance">Account Starting Balance that should be added</param>
    void Add(AccountStartingBalance accountStartingBalance);
}