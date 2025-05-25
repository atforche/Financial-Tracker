using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.BalanceEvents;

/// <summary>
/// Validator class that validates the Event Date for a <see cref="BalanceEvent"/>
/// </summary>
public sealed class BalanceEventDateValidator(IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Validates that the provided Accounting Period and Event Date are valid for a <see cref="BalanceEvent"/>
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Balance Event</param>
    /// <param name="accounts">Accounts for the Balance Event</param>
    /// <param name="eventDate">Event Date for the Balance Event</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the provided Accounting Period and Event Date are valid, false otherwise</returns>
    public bool Validate(
        AccountingPeriod accountingPeriod,
        IEnumerable<Account> accounts,
        DateOnly eventDate,
        [NotNullWhen(false)] out Exception? exception)
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
        foreach (Account account in accounts)
        {
            // Validate that a balance event can only be added after an Account's Added Balance Event
            if (eventDate < account.AccountAddedBalanceEvent.EventDate)
            {
                exception ??= new InvalidOperationException();
            }
            if (accountingPeriod.PeriodStartDate <
                accountingPeriodRepository.FindById(account.AccountAddedBalanceEvent.AccountingPeriodId).PeriodStartDate)
            {
                exception ??= new InvalidOperationException();
            }
        }
        return exception == null;
    }
}