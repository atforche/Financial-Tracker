using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;

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
        List<Account>? accounts = request is not CreateAccountAddedBalanceEventRequest
            ? request.GetAccountIds().Select(accountRepository.FindById).ToList()
            : null;
        if (!ValidateAccountingPeriod(accountingPeriod, accounts, out Exception? exception))
        {
            throw exception;
        }
        if (!ValidateEventDate(request.EventDate, accountingPeriod, accounts, out exception))
        {
            throw exception;
        }
        if (!ValidatePrivate(request, out exception))
        {
            throw exception;
        }
        TBalanceEvent balanceEvent = CreatePrivate(request);
        if (accounts != null)
        {
            if (!ValidateCurrentBalances(balanceEvent, out exception))
            {
                throw exception;
            }
            if (!ValidateFutureBalanceEvents(balanceEvent, out exception))
            {
                throw exception;
            }
            if (!ValidateBalanceEventsByAccountingPeriod(balanceEvent, accounts, out exception))
            {
                throw exception;
            }
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
    /// <param name="accounts">Accounts for this Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Accounting Period is valid for this Balance Event, false otherwise</returns>
    private bool ValidateAccountingPeriod(
        AccountingPeriod accountingPeriod,
        IReadOnlyCollection<Account>? accounts,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (!accountingPeriod.IsOpen)
        {
            exception = new InvalidOperationException();
        }
        if (accounts != null)
        {
            foreach (Account account in accounts)
            {
                if (accountingPeriod.PeriodStartDate <
                    accountingPeriodRepository.FindById(account.AccountAddedBalanceEvent.AccountingPeriodId).PeriodStartDate)
                {
                    // A Balance Event cannot be added to an Accounting Period that falls earlier than the
                    // Account Added Balance Event for the Account
                    exception ??= new InvalidOperationException();
                }
            }
        }
        return exception == null;
    }

    /// <summary>
    /// Validates the Event Date for this Balance Event
    /// </summary>
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <param name="accountingPeriod">Accounting Period for this Balance Event</param>
    /// <param name="accounts">Accounts for this Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Event Date is valid for this Balance Event, false otherwise</returns>
    private static bool ValidateEventDate(
        DateOnly eventDate,
        AccountingPeriod accountingPeriod,
        IReadOnlyCollection<Account>? accounts,
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
        if (accounts != null)
        {
            foreach (Account account in accounts)
            {
                if (eventDate < account.AccountAddedBalanceEvent.EventDate)
                {
                    // A balance event can only be added after an Account's Added Balance Event
                    exception ??= new InvalidOperationException();
                }
            }
        }
        return exception == null;
    }

    /// <summary>
    /// Validates that the Balance Event can be applied to the current Account Balances
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Balance Event can be applied to the current Account Balances, false otherwise</returns>
    private bool ValidateCurrentBalances(
        IBalanceEvent newBalanceEvent,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        foreach (AccountId account in newBalanceEvent.GetAccountIds())
        {
            AccountBalance currentBalance = GetAccountBalanceBeforeEventWasAdded(newBalanceEvent, account);
            _ = newBalanceEvent.IsValidToApplyToBalance(currentBalance, out exception);
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

        foreach (AccountId account in newBalanceEvent.GetAccountIds())
        {
            AccountBalance runningBalance = GetAccountBalanceBeforeEventWasAdded(newBalanceEvent, account);
            runningBalance = newBalanceEvent.ApplyEventToBalance(runningBalance, ApplicationDirection.Standard);
            var futureBalanceEvents = balanceEventRepository
                .FindAllByDateRange(new DateRange(newBalanceEvent.EventDate, DateOnly.MaxValue))
                .Where(balanceEvent => balanceEvent.IsLaterThan(newBalanceEvent))
                .Order(new BalanceEventComparer())
                .ToList();
            foreach (IBalanceEvent balanceEvent in futureBalanceEvents)
            {
                if (!balanceEvent.IsValidToApplyToBalance(runningBalance, out exception))
                {
                    return false;
                }
                runningBalance = balanceEvent.ApplyEventToBalance(runningBalance, ApplicationDirection.Standard);
            }
        }
        return exception == null;
    }

    /// <summary>
    /// Validates that adding this new Balance Event hasn't invalidated any Balance Events within 
    /// the current or any future Accounting Periods
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="accounts">Accounts for the Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if all the future Balance Events are still valid, false otherwise</returns>
    private bool ValidateBalanceEventsByAccountingPeriod(
        IBalanceEvent newBalanceEvent,
        IReadOnlyCollection<Account> accounts,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        AccountingPeriod? previousAccountingPeriod = accountingPeriodRepository.FindPreviousAccountingPeriod(newBalanceEvent.AccountingPeriodId);

        foreach (Account account in accounts)
        {
            // Get the Account Balance as of the beginning of the events Accounting Period
            AccountBalance runningBalance = previousAccountingPeriod != null
                ? accountBalanceService.GetAccountBalanceByAccountingPeriod(account.Id, previousAccountingPeriod.Id).EndingBalance
                : new AccountBalance(account, [], []);

            AccountingPeriod? accountingPeriod = accountingPeriodRepository.FindById(newBalanceEvent.AccountingPeriodId);
            while (accountingPeriod != null)
            {
                // Grab all the Balance Events within the Accounting Period (adding the new Balance Event if necessary)
                var balanceEvents = balanceEventRepository.FindAllByAccountingPeriod(accountingPeriod.Id)
                    .Where(balanceEvent => balanceEvent.GetAccountIds().Contains(account.Id))
                    .ToList();
                if (accountingPeriod.Id == newBalanceEvent.AccountingPeriodId)
                {
                    balanceEvents.Add(newBalanceEvent);
                    balanceEvents.Sort(new BalanceEventComparer());
                }
                // Ensure all the Balance Events can be applied to the Accounting Period balance
                foreach (IBalanceEvent balanceEvent in balanceEvents)
                {
                    if (!balanceEvent.IsValidToApplyToBalance(runningBalance, out exception))
                    {
                        return false;
                    }
                    runningBalance = balanceEvent.ApplyEventToBalance(runningBalance, ApplicationDirection.Standard);
                }
                // Move on to the next Accounting Period
                accountingPeriod = accountingPeriodRepository.FindNextAccountingPeriod(accountingPeriod.Id);
            }
        }
        return exception == null;
    }

    /// <summary>
    /// Gets the Account Balance for the provided Account as it would have been before the provided Balance Event was added
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="accountId">Account to get the balance for</param>
    /// <returns>The Account Balance immediately before the Balance Event was added</returns>
    private AccountBalance GetAccountBalanceBeforeEventWasAdded(IBalanceEvent newBalanceEvent, AccountId accountId)
    {
        // Get the Account Balance as of the date the event was added
        AccountBalance accountBalance = accountBalanceService.GetAccountBalanceByDate(
            accountId,
            newBalanceEvent.EventDate).AccountBalance;

        // Grab all the Balance Events that occurred later on the same date as the new event and reverse them
        var balanceEvents = balanceEventRepository
            .FindAllByDate(newBalanceEvent.EventDate)
            .Where(balanceEvent => balanceEvent.EventSequence > newBalanceEvent.EventSequence
                && balanceEvent.GetAccountIds().Contains(accountId))
            .ToList();
        foreach (IBalanceEvent balanceEvent in balanceEvents)
        {
            accountBalance = balanceEvent.ApplyEventToBalance(accountBalance, ApplicationDirection.Reverse);
        }
        return accountBalance;
    }
}