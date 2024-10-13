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
    /// <param name="accountId">ID of the Account to find the starting balance for</param>
    /// <param name="accountingPeriodId">ID of the accounting period to find the starting balance for</param>
    /// <returns>The Account Starting Balance that was found, or null if one wasn't found</returns>
    AccountStartingBalance? FindOrNull(Guid accountId, Guid accountingPeriodId);

    /// <summary>
    /// Finds all the starting balances for the provided Account
    /// </summary>
    /// <param name="accountId">ID of the Account to find starting balances for</param>
    /// <returns>The list of starting balances for the provided Account</returns>
    ICollection<AccountStartingBalance> FindAllByAccount(Guid accountId);

    /// <summary>
    /// Finds all the starting balances for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to find starting balances for</param>
    /// <returns>The list of starting balances for the provided Accounting Period</returns>
    ICollection<AccountStartingBalance> FindAllByAccountingPeriod(Guid accountingPeriodId);

    /// <summary>
    /// Adds the provided Account Starting Balance to the repository
    /// </summary>
    /// <param name="accountStartingBalance">Account Starting Balance that should be added</param>
    void Add(AccountStartingBalance accountStartingBalance);

    /// <summary>
    /// Deletes the Account Starting Balance with the specified ID
    /// </summary>
    /// <param name="id">ID of the Account Starting Balance to delete</param>
    void Delete(Guid id);
}