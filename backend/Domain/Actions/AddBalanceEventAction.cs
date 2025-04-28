using System.Diagnostics.CodeAnalysis;
using Domain.Aggregates;
using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Actions;

/// <summary>
/// Action class that adds a Balance Event
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
/// <param name="accountBalanceService">Account Balance Service</param>
public abstract class AddBalanceEventAction(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountBalanceService accountBalanceService)
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository = accountingPeriodRepository;
    private readonly IAccountBalanceService _accountBalanceService = accountBalanceService;

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Balance Event</param>
    /// <param name="eventDate">Event Date for the Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    public static bool IsValid(AccountingPeriod accountingPeriod, DateOnly eventDate, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (!accountingPeriod.IsOpen)
        {
            exception = new InvalidOperationException();
        }
        if (eventDate == DateOnly.MinValue)
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that a balance event can only be added with a date in a month adjacent to the Accounting Period month
        int monthDifference = Math.Abs(((accountingPeriod.Year - eventDate.Year) * 12) + accountingPeriod.Month - eventDate.Month);
        if (monthDifference > 1)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates that the newly created Balance Event doesn't invalidate any future balance events
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="accountingPeriod">Accounting Period for the newly created Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the future Balance Events are all still valid, false otherwise</returns>
    protected bool ValidateFutureBalanceEvents(
        BalanceEventBase newBalanceEvent,
        AccountingPeriod accountingPeriod,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        // Validate that this Balance Event is valid to be applied to the current Account Balance
        AccountBalance currentBalance = GetAccountBalanceBeforeEventWasAdded(newBalanceEvent);
        if (!newBalanceEvent.CanBeAppliedToBalance(currentBalance))
        {
            exception = new InvalidOperationException();
        }

        // Validate that adding this Balance Event doesn't cause any of the future Balance Events to become invalid
        AccountBalance runningBalance = newBalanceEvent.ApplyEventToBalance(currentBalance);
        var futureBalanceEventsForAccount = _accountBalanceService
            .GetAccountBalancesByEvent(newBalanceEvent.Account, new DateRange(newBalanceEvent.EventDate, DateOnly.MaxValue))
            .Select(accountBalanceByEvent => accountBalanceByEvent.BalanceEvent)
            .Where(balanceEvent => balanceEvent > newBalanceEvent)
            .Order()
            .ToList();
        foreach (BalanceEventBase balanceEvent in futureBalanceEventsForAccount)
        {
            if (!balanceEvent.CanBeAppliedToBalance(runningBalance))
            {
                exception ??= new InvalidOperationException();
            }
            runningBalance = balanceEvent.ApplyEventToBalance(runningBalance);
        }

        // Validate that adding this Balance Event doesn't cause the balance of this Account within the
        // Accounting Period to become invalid
        runningBalance = _accountBalanceService.GetAccountBalancesByAccountingPeriod(newBalanceEvent.Account, accountingPeriod).StartingBalance;
        foreach (BalanceEventBase balanceEvent in accountingPeriod.GetAllBalanceEvents().Concat([newBalanceEvent]).Order())
        {
            if (!balanceEvent.CanBeAppliedToBalance(runningBalance))
            {
                exception ??= new InvalidOperationException();
            }
            runningBalance = balanceEvent.ApplyEventToBalance(runningBalance);
        }
        return exception == null;
    }

    /// <summary>
    /// Gets the Account Balance as it would have been before the provided Balance Event was added
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <returns>The Account Balance immediately before the Balance Event was added</returns>
    private AccountBalance GetAccountBalanceBeforeEventWasAdded(BalanceEventBase newBalanceEvent)
    {
        // Get the Account Balance as of the date the event was added
        AccountBalance accountBalance = _accountBalanceService.GetAccountBalancesByDate(
            newBalanceEvent.Account,
            new DateRange(newBalanceEvent.EventDate, newBalanceEvent.EventDate)).First().AccountBalance;

        // Grab all the Balance Events that occurred later on the same date as the new event and reverse them
        var balanceEvents = _accountingPeriodRepository
            .FindAccountingPeriodsWithBalanceEventsInDateRange(new DateRange(newBalanceEvent.EventDate, newBalanceEvent.EventDate))
            .SelectMany(accountingPeriod => accountingPeriod.GetAllBalanceEvents())
            .Where(balanceEvent => balanceEvent.EventDate == newBalanceEvent.EventDate && balanceEvent > newBalanceEvent)
            .ToList();
        foreach (BalanceEventBase balanceEvent in balanceEvents)
        {
            accountBalance = balanceEvent.ReverseEventFromBalance(accountBalance);
        }
        return accountBalance;
    }
}