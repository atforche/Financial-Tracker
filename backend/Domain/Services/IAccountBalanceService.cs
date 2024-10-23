using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to calculate the balance of an Account as of a particular point in time
/// </summary>
public interface IAccountBalanceService
{
    /// <summary>
    /// Calculates the balance of the provided Account as of the provided date
    /// </summary>
    /// <param name="account">Account to calculate the balance for</param>
    /// <param name="asOfDate">Date to calculate the balance as of</param>
    /// <returns>The balance of the Account as of the provided date</returns>
    AccountBalance GetAccountBalanceAsOfDate(Account account, DateOnly asOfDate);
}