using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;

namespace Domain.AccountingPeriods;

/// <summary>
/// Factory for building a <see cref="AccountingPeriod"/>
/// </summary>
public class AccountingPeriodFactory(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    AccountBalanceService accountBalanceService)
{
    /// <summary>
    /// Creates a new Accounting Period
    /// </summary>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <returns>The newly create Accounting Period</returns>
    public AccountingPeriod Create(int year, int month)
    {
        if (!ValidateYearAndMonth(year, month, out Exception? exception))
        {
            throw exception;
        }
        if (!ValidateOtherAccountingPeriods(year, month, out exception))
        {
            throw exception;
        }
        var newAccountingPeriod = new AccountingPeriod(year, month);
        AddAccountBalanceCheckpoints(newAccountingPeriod);
        return newAccountingPeriod;
    }

    /// <summary>
    /// Validates the Year and Month for this Accounting Period
    /// </summary>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Year and Month for this Accounting Period are valid, false otherwise</returns>
    private static bool ValidateYearAndMonth(int year, int month, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (year is < 2020 or > 2050)
        {
            exception = new InvalidOperationException();
        }
        if (month is <= 0 or > 12)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates the other Accounting Periods for this Accounting Period
    /// </summary>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the other Accounting Periods are valid, false otherwise</returns>
    private bool ValidateOtherAccountingPeriods(int year, int month, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        // Validate that there are no duplicate accounting periods
        var existingAccountingPeriods = accountingPeriodRepository.FindAll().ToList();
        if (existingAccountingPeriods.Any(period => period.PeriodStartDate == new DateOnly(year, month, 1)))
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that accounting periods can only be added after existing accounting periods
        if (existingAccountingPeriods.Count > 0 &&
            !existingAccountingPeriods.Any(period => period.PeriodStartDate == new DateOnly(year, month, 1).AddMonths(-1)))
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Adds Account Balance Checkpoints for the newly creating Accounting Period if necessary
    /// </summary>
    /// <param name="newAccountingPeriod">Newly created Accounting Period</param>
    private void AddAccountBalanceCheckpoints(AccountingPeriod newAccountingPeriod)
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
}