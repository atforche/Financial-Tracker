using System.Diagnostics.CodeAnalysis;
using Domain.Aggregates;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;

namespace Domain.Actions;

/// <summary>
/// Action class that adds a Fund Conversion
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
/// <param name="accountBalanceService">Account Balance Service</param>
public class AddFundConversionAction(
    IAccountingPeriodRepository accountingPeriodRepository,
    AccountBalanceService accountBalanceService)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Fund Conversion</param>
    /// <param name="eventDate">Event Date for the Fund Conversion</param>
    /// <param name="account">Account for the Fund Conversion</param>
    /// <param name="fromFund">From Fund for the Fund Conversion</param>
    /// <param name="toFund">To Fund for the Fund Conversion</param>
    /// <param name="amount">Amount for the Fund Conversion</param>
    /// <returns>The newly created Fund Conversion</returns>
    public FundConversion Run(
        AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        Fund fromFund,
        Fund toFund,
        decimal amount)
    {
        if (!IsValid(accountingPeriod, eventDate, account, fromFund, toFund, amount, out Exception? exception))
        {
            throw exception;
        }
        var fundConversion = new FundConversion(accountingPeriod, account, eventDate, fromFund, toFund, amount);
        if (!new BalanceEventFutureEventValidator(accountingPeriodRepository, accountBalanceService).Validate(fundConversion, out exception))
        {
            throw exception;
        }
        accountingPeriod.AddFundConversion(fundConversion);
        return fundConversion;
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Fund Conversion</param>
    /// <param name="eventDate">Event Date for the Fund Conversion</param>
    /// <param name="account">Account for the Fund Conversion</param>
    /// <param name="fromFund">From Fund for the Fund Conversion</param>
    /// <param name="toFund">To Fund for the Fund Conversion</param>
    /// <param name="amount">Amount for the Fund Conversion</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private static bool IsValid(
        AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        Fund fromFund,
        Fund toFund,
        decimal amount,
        [NotNullWhen(false)] out Exception? exception)
    {
        if (!new BalanceEventDateValidator(accountingPeriod, [account], eventDate).Validate(out exception))
        {
            return false;
        }
        if (fromFund == toFund)
        {
            exception ??= new InvalidOperationException();
        }
        if (amount <= 0)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}