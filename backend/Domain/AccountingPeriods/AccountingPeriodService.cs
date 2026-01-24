using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods.Exceptions;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Periods
/// </summary>
public class AccountingPeriodService(IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Attempts to create a new Accounting Period
    /// </summary>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <param name="accountingPeriod">The created Accounting Period, or null if creation failed</param>
    /// <param name="exceptions">List of exceptions encountered during creation</param>
    /// <returns>True if the Accounting Period was created successfully, false otherwise</returns>
    public bool TryCreate(int year, int month, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        accountingPeriod = null;

        if (!ValidateCreate(year, month, out exceptions))
        {
            return false;
        }
        accountingPeriod = new AccountingPeriod(year, month);
        return true;
    }

    /// <summary>
    /// Attempts to close an existing Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to close</param>
    /// <param name="exceptions">List of exceptions encountered during closing</param>
    /// <returns>True if the Accounting Period was closed successfully, false otherwise</returns>
    public bool TryClose(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateClose(accountingPeriod, out exceptions))
        {
            return false;
        }
        accountingPeriod.IsOpen = false;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to delete</param>
    /// <param name="exceptions">List of exceptions encountered during deletion</param>
    /// <returns>True if the Accounting Period was deleted successfully, false otherwise</returns>
    public bool TryDelete(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(accountingPeriod, out exceptions))
        {
            return false;
        }
        accountingPeriodRepository.Delete(accountingPeriod);
        return true;
    }

    /// <summary>
    /// Validates creating a new Accounting Period
    /// </summary>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if the Accounting Period can be created, false otherwise</returns>
    private bool ValidateCreate(int year, int month, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (year is < 2020 or > 2050)
        {
            exceptions = exceptions.Append(new InvalidYearException());
        }
        if (month is <= 0 or > 12)
        {
            exceptions = exceptions.Append(new InvalidMonthException());
        }
        if (exceptions.Any())
        {
            // If year or month are invalid, no need to continue validation
            return false;
        }
        // Validate that there are no duplicate accounting periods
        var existingAccountingPeriods = accountingPeriodRepository.FindAll().ToList();
        if (existingAccountingPeriods.Any(period => period.PeriodStartDate == new DateOnly(year, month, 1)))
        {
            exceptions = exceptions.Append(new DuplicateAccountingPeriodException());
        }
        // Validate that accounting periods can only be added after existing accounting periods
        if (existingAccountingPeriods.Count > 0 &&
            !existingAccountingPeriods.Any(period => period.PeriodStartDate == new DateOnly(year, month, 1).AddMonths(-1)))
        {
            exceptions = exceptions.Append(new NonSequentialAccountingPeriodException());
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates closing an existing Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to close</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if the Accounting Period can be closed, false otherwise</returns>
    private bool ValidateClose(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new AccountingPeriodIsClosedException());
        }
        // Validate that there are no other earlier open Accounting Periods
        if (accountingPeriodRepository.FindAllOpenPeriods().Any(openPeriod => openPeriod.PeriodStartDate < accountingPeriod.PeriodStartDate))
        {
            exceptions = exceptions.Append(new EarlierAccountingPeriodStillOpenException());
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates deleting an existing Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to delete</param>
    /// <param name="exceptions">List of exceptions encountered during deletion</param>
    /// <returns>True if the Accounting Period can be deleted, false otherwise</returns>
    private bool ValidateDelete(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new AccountingPeriodIsClosedException());
        }
        if (accountingPeriodRepository.FindNextAccountingPeriod(accountingPeriod.Id) != null)
        {
            exceptions = exceptions.Append(new AccountingPeriodGapException());
        }
        return !exceptions.Any();
    }
}