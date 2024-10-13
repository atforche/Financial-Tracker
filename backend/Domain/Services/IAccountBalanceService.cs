using Domain.Entities;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to calculate the balance of an Account as of a
/// particular date or transaction
/// </summary>
public interface IAccountBalanceService
{
    /// <summary>
    /// Calculates the balance of the provided account as of the provided date
    /// </summary>
    /// <param name="account">Account to calculate the balance for</param>
    /// <param name="asOfDate">Date to calculate the balance as of</param>
    /// <returns>The balance of the Account as of the provided date</returns>
    AccountBalance GetAccountBalanceAsOfDate(Account account, DateOnly asOfDate);
}

/// <summary>
/// Balance of a particular account
/// </summary>
/// <param name="Balance">Current balance of the Account</param>
/// <param name="BalanceIncludingPendingTransactions">Balance of the Account including any pending transactions</param>
public record AccountBalance(decimal Balance, decimal BalanceIncludingPendingTransactions);