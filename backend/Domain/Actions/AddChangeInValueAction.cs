using System.Diagnostics.CodeAnalysis;
using Domain.Aggregates;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Actions;

/// <summary>
/// Action class that adds a Change in Value
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
/// <param name="accountBalanceService">Account Balance Service</param>
public class AddChangeInValueAction(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountBalanceService accountBalanceService)
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
        if (!IsValid(accountingPeriod, eventDate, accountingEntry, out Exception? exception))
        {
            throw exception;
        }
        var changeInValue = new ChangeInValue(accountingPeriod, account, eventDate, accountingEntry);
        if (!new BalanceEventFutureEventValidator(accountingPeriodRepository, accountBalanceService).Validate(changeInValue, out exception))
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
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private static bool IsValid(
        AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        FundAmount accountingEntry,
        [NotNullWhen(false)] out Exception? exception)
    {
        if (!new BalanceEventAccountingPeriodValidator(accountingPeriod, eventDate).Validate(out exception))
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