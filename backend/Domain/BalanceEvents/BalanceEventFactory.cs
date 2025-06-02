using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Services;

namespace Domain.BalanceEvents;

/// <summary>
/// Factory for building a <see cref="IBalanceEvent"/>
/// </summary>
public abstract class BalanceEventFactory<TBalanceEvent, TRequest>(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
    where TBalanceEvent : IBalanceEvent
    where TRequest : CreateBalanceEventRequest
{
    /// <summary>
    /// Create a new Balance Event
    /// </summary>
    /// <param name="request">Request to create a Balance Event</param>
    /// <returns>The newly created Balance Event</returns>
    public TBalanceEvent Create(TRequest request)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(request.AccountingPeriodId);
        Account? account = request is not CreateAccountAddedBalanceEventRequest
            ? accountRepository.FindById(request.AccountId)
            : null;
        if (!ValidateAccountingPeriod(accountingPeriod, account, out Exception? exception))
        {
            throw exception;
        }
        if (!ValidateEventDate(request.EventDate, accountingPeriod, account, out exception))
        {
            throw exception;
        }
        if (!ValidatePrivate(request, out exception))
        {
            throw exception;
        }
        TBalanceEvent balanceEvent = CreatePrivate(request);
        if (account != null)
        {
            if (!ValidateCurrentBalance(balanceEvent, out exception))
            {
                throw exception;
            }
            if (!ValidateFutureBalanceEvents(balanceEvent, out exception))
            {
                throw exception;
            }
            if (!ValidateBalanceEventsByAccountingPeriod(balanceEvent, account, out exception))
            {
                throw exception;
            }
        }
        if (balanceEvent is ChangeInValue changeInValue)
        {
            accountingPeriod.AddChangeInValue(changeInValue);
        }

        return balanceEvent;
    }

    /// <summary>
    /// Creates a new Balance Event
    /// </summary>
    /// <param name="request">Request to create a Balance Event</param>
    /// <returns>The newly created Balance Event</returns>
    protected abstract TBalanceEvent CreatePrivate(TRequest request);

    /// <summary>
    /// Validates a request to create a Balance Event
    /// </summary>
    /// <param name="request">Request to create a Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    protected abstract bool ValidatePrivate(TRequest request, [NotNullWhen(false)] out Exception? exception);

    /// <summary>
    /// Gets the Sequence for this Balance Event
    /// </summary>
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <returns>The Sequence for this Balance Event</returns>
    protected int GetBalanceEventSequence(DateOnly eventDate) =>
        balanceEventRepository.GetHighestEventSequenceOnDate(eventDate) + 1;

    /// <summary>
    /// Validates the Accounting Period for this Balance Event
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for this Balance Event</param>
    /// <param name="account">Account for this Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Accounting Period is valid for this Balance Event, false otherwise</returns>
    private bool ValidateAccountingPeriod(
        AccountingPeriod accountingPeriod,
        Account? account,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (!accountingPeriod.IsOpen)
        {
            exception = new InvalidOperationException();
        }
        if (account != null &&
            accountingPeriod.PeriodStartDate <
            accountingPeriodRepository.FindById(account.AccountAddedBalanceEvent.AccountingPeriodId).PeriodStartDate)
        {
            // A Balance Event cannot be added to an Accounting Period that falls earlier than the
            // Account Added Balance Event for the Account
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates the Event Date for this Balance Event
    /// </summary>
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <param name="accountingPeriod">Accounting Period for this Balance Event</param>
    /// <param name="account">Account for this Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Event Date is valid for this Balance Event, false otherwise</returns>
    private static bool ValidateEventDate(
        DateOnly eventDate,
        AccountingPeriod accountingPeriod,
        Account? account,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (eventDate == DateOnly.MinValue)
        {
            exception = new InvalidOperationException();
        }
        int monthDifference = Math.Abs(((accountingPeriod.Year - eventDate.Year) * 12) + accountingPeriod.Month - eventDate.Month);
        if (monthDifference > 1)
        {
            // A balance event can only be added with a date in a month adjacent to the Accounting Period month
            exception ??= new InvalidOperationException();
        }
        if (account != null && eventDate < account.AccountAddedBalanceEvent.EventDate)
        {
            // A balance event can only be added after an Account's Added Balance Event
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates that the Balance Event can be applied to the current Account Balance
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Balance Event can be applied to the current Account Balance, false otherwise</returns>
    private bool ValidateCurrentBalance(
        IBalanceEvent newBalanceEvent,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        AccountBalance currentBalance = GetAccountBalanceBeforeEventWasAdded(newBalanceEvent);
        if (!newBalanceEvent.CanBeAppliedToBalance(currentBalance))
        {
            exception = new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates that adding this new Balance Event hasn't invalidated any future Balance Events
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="exception">Exception encountered validation</param>
    /// <returns>True if all the future Balance Events are still valid, false otherwise</returns>
    private bool ValidateFutureBalanceEvents(
        IBalanceEvent newBalanceEvent,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        AccountBalance runningBalance = GetAccountBalanceBeforeEventWasAdded(newBalanceEvent);
        runningBalance = newBalanceEvent.ApplyEventToBalance(runningBalance);
        var futureBalanceEvents = balanceEventRepository
            .FindAllByDateRange(new DateRange(newBalanceEvent.EventDate, DateOnly.MaxValue))
            .Where(balanceEvent => balanceEvent.IsLaterThan(newBalanceEvent))
            .Order(new BalanceEventComparer())
            .ToList();
        foreach (IBalanceEvent balanceEvent in futureBalanceEvents)
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
    /// Validates that adding this new Balance Event hasn't invalidated any Balance Events within 
    /// the current or any future Accounting Periods
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="account">Account for the Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if all the future Balance Events are still valid, false otherwise</returns>
    private bool ValidateBalanceEventsByAccountingPeriod(
        IBalanceEvent newBalanceEvent,
        Account account,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        // Get the Account Balance as of the beginning of the events Accounting Period
        AccountingPeriod? previousAccountingPeriod = accountingPeriodRepository.FindPreviousAccountingPeriod(newBalanceEvent.AccountingPeriodId);
        AccountBalance runningBalance = previousAccountingPeriod != null
            ? accountBalanceService.GetAccountBalanceByAccountingPeriod(account.Id, previousAccountingPeriod.Id).EndingBalance
            : new AccountBalance(account, [], []);

        AccountingPeriod? accountingPeriod = accountingPeriodRepository.FindById(newBalanceEvent.AccountingPeriodId);
        while (accountingPeriod != null)
        {
            // Grab all the Balance Events within the Accounting Period (adding the new Balance Event if necessary)
            var balanceEvents = balanceEventRepository.FindAllByAccountingPeriod(accountingPeriod.Id)
                .Where(balanceEvent => balanceEvent.AccountId == newBalanceEvent.AccountId)
                .ToList();
            if (accountingPeriod.Id == newBalanceEvent.AccountingPeriodId)
            {
                balanceEvents.Add(newBalanceEvent);
                balanceEvents.Sort(new BalanceEventComparer());
            }
            // Ensure all the Balance Events can be applied to the Accounting Period balance
            foreach (IBalanceEvent balanceEvent in balanceEvents)
            {
                if (!balanceEvent.CanBeAppliedToBalance(runningBalance))
                {
                    exception ??= new InvalidOperationException();
                }
                runningBalance = balanceEvent.ApplyEventToBalance(runningBalance);
            }
            // Move on to the next Accounting Period
            accountingPeriod = accountingPeriodRepository.FindNextAccountingPeriod(accountingPeriod.Id);
        }
        return exception == null;
    }

    /// <summary>
    /// Gets the Account Balance as it would have been before the provided Balance Event was added
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <returns>The Account Balance immediately before the Balance Event was added</returns>
    private AccountBalance GetAccountBalanceBeforeEventWasAdded(IBalanceEvent newBalanceEvent)
    {
        // Get the Account Balance as of the date the event was added
        AccountBalance accountBalance = accountBalanceService.GetAccountBalanceByDate(
            newBalanceEvent.AccountId,
            newBalanceEvent.EventDate).AccountBalance;

        // Grab all the Balance Events that occurred later on the same date as the new event and reverse them
        var balanceEvents = balanceEventRepository
            .FindAllByDate(newBalanceEvent.EventDate)
            .Where(balanceEvent => balanceEvent.EventSequence > newBalanceEvent.EventSequence)
            .ToList();
        foreach (IBalanceEvent balanceEvent in balanceEvents)
        {
            accountBalance = balanceEvent.ReverseEventFromBalance(accountBalance);
        }
        return accountBalance;
    }
}