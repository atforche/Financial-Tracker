using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.Actions;

/// <summary>
/// Action class that adds a Fund Conversion
/// </summary>
/// <param name="balanceEventRepository">Balance Event Repository</param>
/// <param name="balanceEventDateValidator">Balance Event Date Validator</param>
/// <param name="balanceEventFutureEventValidator">Balance Event Future Event Validator</param>
public class AddFundConversionAction(
    IBalanceEventRepository balanceEventRepository,
    BalanceEventDateValidator balanceEventDateValidator,
    BalanceEventFutureEventValidator balanceEventFutureEventValidator)
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
        var fundConversion = new FundConversion(accountingPeriod.Id,
            eventDate,
            balanceEventRepository.GetHighestEventSequenceOnDate(eventDate) + 1,
            account,
            fromFund,
            toFund,
            amount);
        if (!balanceEventFutureEventValidator.Validate(fundConversion, out exception))
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
    private bool IsValid(
        AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        Fund fromFund,
        Fund toFund,
        decimal amount,
        [NotNullWhen(false)] out Exception? exception)
    {
        if (!balanceEventDateValidator.Validate(accountingPeriod, [account], eventDate, out exception))
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