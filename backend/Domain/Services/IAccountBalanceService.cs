using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to calculate the balance of an Account as of a particular point in time
/// </summary>
public interface IAccountBalanceService
{
    /// <summary>
    /// Gets the list of Account Balances for each date over the provided Date Range
    /// </summary>
    /// <param name="account">Account to get the balances for</param>
    /// <param name="dateRange">Date Range to get the daily balances for</param>
    /// <returns>The list of Account Balances for each date over the Date Range</returns>
    IReadOnlyCollection<AccountBalanceByDate> GetAccountBalancesByDate(
        Account account,
        DateRange dateRange);

    /// <summary>
    /// Gets the list of Account Balances for each Balance Event over the provided Date Range
    /// </summary>
    /// <param name="account">Account to get the balances for</param>
    /// <param name="dateRange">Date Range to get the event balances for</param>
    /// <returns>The list of Account Balances for each Balance Event over the Date Range</returns>
    IReadOnlyCollection<AccountBalanceByEvent> GetAccountBalancesByEvent(
        Account account,
        DateRange dateRange);

    /// <summary>
    /// Gets the Account Balances at the beginning and end of the provided Accounting Period.
    /// </summary>
    /// <param name="account">Account to get the balances for</param>
    /// <param name="accountingPeriod">Accounting Period to get the </param>
    /// <returns>The beginning and ending Account Balances for the Accounting Period</returns>
    AccountBalanceByAccountingPeriod GetAccountBalancesByAccountingPeriod(
        Account account,
        AccountingPeriod accountingPeriod);
}