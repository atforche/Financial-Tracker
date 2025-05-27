using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Services;

namespace Domain.BalanceEvents;

/// <summary>
/// Validator class that validates that a newly created Balance Event doesn't invalidate any future Balance Events
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
/// <param name="balanceEventRepository">Balance Event Repository</param>
/// <param name="accountBalanceService">Account Balance Service</param>
public sealed class BalanceEventFutureEventValidator(
    IAccountingPeriodRepository accountingPeriodRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
{
    /// <summary>
    /// Validates that the newly created Balance Event doesn't invalidate any future Balance Events
    /// </summary>
    /// <param name="newBalanceEvent">The newly created Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the future Balance Events are still valid, false otherwise</returns>
    public bool Validate(BalanceEvent newBalanceEvent, [NotNullWhen(false)] out Exception? exception)
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
        var futureBalanceEventsForAccount = balanceEventRepository
            .FindAllByDateRange(new DateRange(newBalanceEvent.EventDate, DateOnly.MaxValue))
            .Where(balanceEvent => balanceEvent.Account == newBalanceEvent.Account && balanceEvent > newBalanceEvent)
            .Order()
            .ToList();
        foreach (BalanceEvent balanceEvent in futureBalanceEventsForAccount)
        {
            if (!balanceEvent.CanBeAppliedToBalance(runningBalance))
            {
                exception ??= new InvalidOperationException();
            }
            runningBalance = balanceEvent.ApplyEventToBalance(runningBalance);
        }

        // Validate that adding this Balance Event doesn't cause the balance of this Account within the
        // Accounting Period to become invalid
        AccountingPeriod eventAccountingPeriod = accountingPeriodRepository.FindById(newBalanceEvent.AccountingPeriodId);
        AccountingPeriod? previousAccountingPeriod = accountingPeriodRepository.FindByDateOrNull(eventAccountingPeriod.PeriodStartDate.AddMonths(-1));
        runningBalance = previousAccountingPeriod != null
            ? accountBalanceService.GetAccountBalancesByAccountingPeriod(newBalanceEvent.Account, previousAccountingPeriod).EndingBalance
            : new AccountBalance(newBalanceEvent.Account, [], []);
        foreach (BalanceEvent balanceEvent in balanceEventRepository.FindAllByAccountingPeriod(eventAccountingPeriod.Id)
                    .Where(balanceEvent => balanceEvent.Account == newBalanceEvent.Account)
                    .Concat([newBalanceEvent])
                    .Order())
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
    private AccountBalance GetAccountBalanceBeforeEventWasAdded(BalanceEvent newBalanceEvent)
    {
        // Get the Account Balance as of the date the event was added
        AccountBalance accountBalance = accountBalanceService.GetAccountBalancesByDate(
            newBalanceEvent.Account,
            new DateRange(newBalanceEvent.EventDate, newBalanceEvent.EventDate)).First().AccountBalance;

        // Grab all the Balance Events that occurred later on the same date as the new event and reverse them
        var balanceEvents = balanceEventRepository
            .FindAllByDateRange(new DateRange(newBalanceEvent.EventDate, newBalanceEvent.EventDate))
            .Where(balanceEvent => balanceEvent.EventDate == newBalanceEvent.EventDate && balanceEvent > newBalanceEvent)
            .ToList();
        foreach (BalanceEvent balanceEvent in balanceEvents)
        {
            accountBalance = balanceEvent.ReverseEventFromBalance(accountBalance);
        }
        return accountBalance;
    }
}