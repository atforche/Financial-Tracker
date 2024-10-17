using Domain.Entities;

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

/// <summary>
/// Balance of an Account
/// </summary>
/// <remarks>
/// A Transaction is considered pending against an Account in the following situations:
/// 1. The Transaction has not been posted yet
/// 2. The Transaction has been posted, however the period in time of this balance falls between the 
///     Transaction's accounting date and statement date
/// </remarks>
/// <param name="Balance">Current balance of the Account</param>
/// <param name="BalanceIncludingPendingTransactions">Balance of the Account including any pending Transactions</param>
public record AccountBalance(decimal Balance, decimal BalanceIncludingPendingTransactions);