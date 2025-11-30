using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents.Exceptions;

namespace Domain.BalanceEvents;

/// <summary>
/// Factory for building a <see cref="IBalanceEvent"/>
/// </summary>
public abstract class BalanceEventFactory<TBalanceEvent, TRequest>(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
    where TBalanceEvent : class, IBalanceEvent
    where TRequest : CreateBalanceEventRequest
{
    /// <summary>
    /// Attempts to create a new Balance Event
    /// </summary>
    /// <param name="request">Request to create a Balance Event</param>
    /// <param name="balanceEvent">The newly created Balance Event, or null if creation failed</param>
    /// <param name="exceptions">Exceptions encountered during creation</param>
    /// <returns>True if the Balance Event was successfully created, false otherwise</returns>
    public bool TryCreate(TRequest request, [NotNullWhen(true)] out TBalanceEvent? balanceEvent, out IEnumerable<Exception> exceptions)
    {
        balanceEvent = null;
        exceptions = [];

        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(request.AccountingPeriodId);
        List<Account>? accounts = request is not CreateAccountAddedBalanceEventRequest
            ? request.GetAccountIds().Select(accountRepository.FindById).ToList()
            : null;
        if (!ValidateAccountingPeriod(accountingPeriod, accounts, out IEnumerable<Exception>? accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (!ValidateEventDate(request.EventDate, accountingPeriod, accounts, out IEnumerable<Exception> eventDateExceptions))
        {
            exceptions = exceptions.Concat(eventDateExceptions);
        }
        if (!ValidatePrivate(request, out IEnumerable<Exception> privateExceptions))
        {
            exceptions = exceptions.Concat(privateExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        balanceEvent = CreatePrivate(request);
        if (accounts != null)
        {
            if (!ValidateCurrentBalances(balanceEvent, out IEnumerable<Exception> currentBalanceExceptions))
            {
                exceptions = exceptions.Concat(currentBalanceExceptions);
            }
            if (!ValidateFutureBalanceEvents(balanceEvent, out IEnumerable<Exception> futureBalanceEventExceptions))
            {
                exceptions = exceptions.Concat(futureBalanceEventExceptions);
            }
            if (!ValidateBalanceEventsByAccountingPeriod(balanceEvent, accounts, out IEnumerable<Exception> accountingPeriodBalanceExceptions))
            {
                exceptions = exceptions.Concat(accountingPeriodBalanceExceptions);
            }
        }
        if (exceptions.Any())
        {
            balanceEvent = null;
            return false;
        }
        return true;
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
    /// <param name="exceptions">Exceptions encountered during validation</param>
    protected abstract bool ValidatePrivate(TRequest request, out IEnumerable<Exception> exceptions);

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
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if the Accounting Period is valid for this Balance Event, false otherwise</returns>
    private bool ValidateAccountingPeriod(
        AccountingPeriod accountingPeriod,
        IReadOnlyCollection<Account>? accounts,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Accounting Period is closed."));
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
                    exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Account did not exist during the provided Accounting Period."));
                }
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Event Date for this Balance Event
    /// </summary>
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <param name="accountingPeriod">Accounting Period for this Balance Event</param>
    /// <param name="accounts">Accounts for this Balance Event</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if the Event Date is valid for this Balance Event, false otherwise</returns>
    private static bool ValidateEventDate(
        DateOnly eventDate,
        AccountingPeriod accountingPeriod,
        IReadOnlyCollection<Account>? accounts,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (eventDate == DateOnly.MinValue)
        {
            exceptions = exceptions.Append(new InvalidEventDateException("The Balance Event must have a valid Event Date."));
        }
        int monthDifference = Math.Abs(((accountingPeriod.Year - eventDate.Year) * 12) + accountingPeriod.Month - eventDate.Month);
        if (monthDifference > 1)
        {
            // A balance event can only be added with a date in a month adjacent to the Accounting Period month
            exceptions = exceptions.Append(new InvalidEventDateException("The Event Date must be in a month adjacent to the Accounting Period month."));
        }
        if (accounts != null)
        {
            foreach (Account account in accounts)
            {
                if (eventDate < account.AccountAddedBalanceEvent.EventDate)
                {
                    // A balance event can only be added after an Account's Added Balance Event
                    exceptions = exceptions.Append(new InvalidEventDateException("The Account did not exist on the provided Event Date."));
                }
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates that the Balance Event can be applied to the current Account Balances
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if the Balance Event can be applied to the current Account Balances, false otherwise</returns>
    private bool ValidateCurrentBalances(IBalanceEvent newBalanceEvent, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (AccountId account in newBalanceEvent.GetAccountIds())
        {
            AccountBalance currentBalance = GetAccountBalanceBeforeEventWasAdded(newBalanceEvent, account);
            if (!newBalanceEvent.IsValidToApplyToBalance(currentBalance, out IEnumerable<Exception> accountExceptions))
            {
                exceptions = exceptions.Concat(accountExceptions);
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates that adding this new Balance Event hasn't invalidated any future Balance Events
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="exceptions">Exceptions encountered validation</param>
    /// <returns>True if all the future Balance Events are still valid, false otherwise</returns>
    private bool ValidateFutureBalanceEvents(IBalanceEvent newBalanceEvent, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

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
                if (!balanceEvent.IsValidToApplyToBalance(runningBalance, out exceptions))
                {
                    return false;
                }
                runningBalance = balanceEvent.ApplyEventToBalance(runningBalance, ApplicationDirection.Standard);
            }
        }
        return true;
    }

    /// <summary>
    /// Validates that adding this new Balance Event hasn't invalidated any Balance Events within 
    /// the current or any future Accounting Periods
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="accounts">Accounts for the Balance Event</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if all the future Balance Events are still valid, false otherwise</returns>
    private bool ValidateBalanceEventsByAccountingPeriod(
        IBalanceEvent newBalanceEvent,
        IReadOnlyCollection<Account> accounts,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];
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
                    if (!balanceEvent.IsValidToApplyToBalance(runningBalance, out exceptions))
                    {
                        return false;
                    }
                    runningBalance = balanceEvent.ApplyEventToBalance(runningBalance, ApplicationDirection.Standard);
                }
                // Move on to the next Accounting Period
                accountingPeriod = accountingPeriodRepository.FindNextAccountingPeriod(accountingPeriod.Id);
            }
        }
        return true;
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