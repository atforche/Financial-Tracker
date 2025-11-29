using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods.Exceptions;
using Domain.Accounts;
using Domain.BalanceEvents;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Periods
/// </summary>
public class AccountingPeriodService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
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
        AddAccountBalanceCheckpointsForNewAccountingPeriod(accountingPeriod);
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
        AddAccountBalanceCheckpointsForClosedAccountingPeriod(accountingPeriod);
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
            exceptions = exceptions.Append(new AccountingPeriodGapException());
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
        // Validate that there are no pending balance changes in this Accounting Period
        if (accountRepository.FindAll()
                .Select(account => accountBalanceService.GetAccountBalanceByAccountingPeriod(account.Id, accountingPeriod.Id))
                .Any(balance => balance.EndingBalance.PendingFundBalanceChanges.Count != 0))
        {
            exceptions = exceptions.Append(new AccountingPeriodHasPendingBalanceChangesException());
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
        if (balanceEventRepository.FindAllByAccountingPeriod(accountingPeriod.Id).Count != 0)
        {
            exceptions = exceptions.Append(new AccountingPeriodHasBalanceEventsException());
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Adds Account Balance Checkpoints for the newly creating Accounting Period if necessary
    /// </summary>
    /// <param name="newAccountingPeriod">Newly created Accounting Period</param>
    private void AddAccountBalanceCheckpointsForNewAccountingPeriod(AccountingPeriod newAccountingPeriod)
    {
        AccountingPeriod? previousAccountingPeriod = accountingPeriodRepository.FindLatestAccountingPeriod();
        if (previousAccountingPeriod == null || previousAccountingPeriod.IsOpen)
        {
            return;
        }
        foreach (Account account in accountRepository.FindAll())
        {
            account.AddAccountBalanceCheckpoint(newAccountingPeriod.Id,
                accountBalanceService.GetAccountBalanceByAccountingPeriod(account.Id, previousAccountingPeriod.Id).EndingBalance.FundBalances);
        }
    }

    /// <summary>
    /// Adds Account Balance Checkpoints for the future Accounting Period if necessary
    /// </summary>
    /// <param name="closedAccountingPeriod">Accounting Period to close</param>
    private void AddAccountBalanceCheckpointsForClosedAccountingPeriod(AccountingPeriod closedAccountingPeriod)
    {
        AccountingPeriod? futureAccountingPeriod = accountingPeriodRepository.FindNextAccountingPeriod(closedAccountingPeriod.Id);
        if (futureAccountingPeriod == null)
        {
            return;
        }
        foreach (Account account in accountRepository.FindAll())
        {
            account.AddAccountBalanceCheckpoint(futureAccountingPeriod.Id,
                accountBalanceService.GetAccountBalanceByAccountingPeriod(account.Id, closedAccountingPeriod.Id).EndingBalance.FundBalances);
        }
    }
}