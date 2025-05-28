using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.Actions;

/// <summary>
/// Action class that adds a Change in Value
/// </summary>
/// <param name="balanceEventRepository">Balance Event Repository</param>
/// <param name="balanceEventDateValidator">Balance Event Date Validator</param>
/// <param name="balanceEventFutureEventValidator">Balance Event Future Event Validator</param>
public class AddChangeInValueAction(
    IBalanceEventRepository balanceEventRepository,
    BalanceEventDateValidator balanceEventDateValidator,
    BalanceEventFutureEventValidator balanceEventFutureEventValidator)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Change in Value</param>
    /// <param name="eventDate">Event Date for the Change in Value</param>
    /// <param name="account">Account for the Change in Value</param>
    /// <param name="accountingEntry">Accounting Entry for the Change in Value</param>
    /// <returns>The newly created Change in Value</returns>
    public ChangeInValue Run(
        AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        FundAmount accountingEntry)
    {
        if (!IsValid(accountingPeriod, eventDate, account, accountingEntry, out Exception? exception))
        {
            throw exception;
        }
        var changeInValue = new ChangeInValue(accountingPeriod.Id,
            eventDate,
            balanceEventRepository.GetHighestEventSequenceOnDate(eventDate) + 1,
            account,
            accountingEntry);
        if (!balanceEventFutureEventValidator.Validate(changeInValue, out exception))
        {
            throw exception;
        }
        accountingPeriod.AddChangeInValue(changeInValue);
        return changeInValue;
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Change in Value</param>
    /// <param name="eventDate">Event Date for the Change in Value</param>
    /// <param name="accountingEntry">Accounting Entry for the Change in Value</param>
    /// <param name="account">Account for the Change in Value</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private bool IsValid(
        AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        FundAmount accountingEntry,
        [NotNullWhen(false)] out Exception? exception)
    {
        if (!balanceEventDateValidator.Validate(accountingPeriod, [account], eventDate, out exception))
        {
            return false;
        }
        if (accountingEntry.Amount == 0)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}