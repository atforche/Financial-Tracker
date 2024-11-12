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
    IReadOnlyCollection<AccountBalanceByDate> GetAccountBalancesForDateRange(
        Account account,
        DateRange dateRange);

    /// <summary>
    /// Gets the list of Account Balances for each Balance Event over the provided Date Range
    /// </summary>
    /// <param name="account">Account to get the balances for</param>
    /// <param name="dateRange">Date Range to get the event balances for</param>
    /// <returns>The list of Account Balances for each Balance Event over the Date Range</returns>
    IReadOnlyCollection<AccountBalanceByEvent> GetAccountBalancesForEvents(
        Account account,
        DateRange dateRange);

    /// <summary>
    /// Gets the Account Balances at the beginning and end of the provided Accounting Period.
    /// </summary>
    /// <param name="account">Account to get the balances for</param>
    /// <param name="accountingPeriod">Accounting Period to get the </param>
    /// <returns>The beginning and ending Account Balances for the Accounting Period</returns>
    AccountBalanceByAccountingPeriod GetAccountBalancesForAccountingPeriod(
        Account account,
        AccountingPeriod accountingPeriod);
}

/// <summary>
/// Record class representing an Account Balance associated with a Date
/// </summary>
/// <param name="Date">Date for this Account Balance</param>
/// <param name="AccountBalance">Account Balance</param>
public record AccountBalanceByDate(DateOnly Date, AccountBalance AccountBalance);

/// <summary>
/// Record class representing an Account Balance associated with a Balance Event
/// </summary>
/// <param name="BalanceEvent">Balance Event for this Account Balance</param>
/// <param name="AccountBalance">Account Balance</param>
public record AccountBalanceByEvent(IBalanceEvent BalanceEvent, AccountBalance AccountBalance);

/// <summary>
/// Record class representing the starting and ending balances across an Accounting Period
/// </summary>
/// <param name="StartingBalance">Starting Account Balance for this Accounting Period</param>
/// <param name="EndingBalance">Ending Account Balance for this Accounting Period</param>
public record AccountBalanceByAccountingPeriod(AccountBalance StartingBalance, AccountBalance EndingBalance);