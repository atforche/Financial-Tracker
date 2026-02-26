using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods.Exceptions;
using Domain.Transactions;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Periods
/// </summary>
public class AccountingPeriodService(IAccountingPeriodRepository accountingPeriodRepository, ITransactionRepository transactionRepository)
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
        if (accountingPeriodRepository.GetByYearAndMonth(year, month) != null)
        {
            exceptions = exceptions.Append(new InvalidMonthException("This Accounting Period already exists."));
        }
        // Validate that accounting periods can only be added after existing accounting periods
        AccountingPeriod? latestAccountingPeriod = accountingPeriodRepository.GetLatestAccountingPeriod();
        if (latestAccountingPeriod != null && latestAccountingPeriod.PeriodStartDate != new DateOnly(year, month, 1).AddMonths(-1))
        {
            exceptions = exceptions.Append(new InvalidMonthException("New Accounting Period must directly follow the most recent existing Accounting Period."));
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
            exceptions = exceptions.Append(new UnableToCloseException("This Accounting Period is already closed."));
        }
        if (transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id).Any(transaction =>
            (transaction.DebitAccount != null && transaction.DebitAccount.PostedDate == null) ||
            (transaction.CreditAccount != null && transaction.CreditAccount.PostedDate == null)))
        {
            exceptions = exceptions.Append(new UnableToCloseException("There are unposted transactions in this Accounting Period."));
        }
        if (accountingPeriodRepository.GetAllOpenPeriods().Any(openPeriod => openPeriod.PeriodStartDate < accountingPeriod.PeriodStartDate))
        {
            exceptions = exceptions.Append(new UnableToCloseException("An earlier Accounting Period is still open."));
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
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period is closed."));
        }
        if (transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id).Count > 0)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period has transactions."));
        }
        if (accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id) != null)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Deleting this Accounting Period would cause a gap between existing Accounting Periods."));
        }
        return !exceptions.Any();
    }
}