using System.Diagnostics.CodeAnalysis;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;

namespace Domain.Actions;

/// <summary>
/// Action class that adds an Accounting Period
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
/// <param name="accountRepository">Account Repository</param>
/// <param name="accountBalanceService">Account Balance Service</param>
public class AddAccountingPeriodAction(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    AccountBalanceService accountBalanceService)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <returns>The Accounting Period that was added</returns>
    public AccountingPeriod Run(int year, int month)
    {
        if (!IsValid(year, month, out Exception? exception))
        {
            throw exception;
        }
        var newAccountingPeriod = new AccountingPeriod(year, month);
        AddAccountBalanceCheckpoints(newAccountingPeriod);
        return newAccountingPeriod;
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="year">Year for the Accounting Period</param>
    /// <param name="month">Month for the Accounting Period</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private bool IsValid(int year, int month, [NotNullWhen(false)] out Exception? exception)
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
        AccountingPeriod? previousAccountingPeriod =
            accountingPeriodRepository.FindByDateOrNull(newAccountingPeriod.PeriodStartDate.AddMonths(-1));
        if (previousAccountingPeriod == null || previousAccountingPeriod.IsOpen)
        {
            return;
        }
        foreach (Account account in accountRepository.FindAll())
        {
            account.AddAccountBalanceCheckpoint(newAccountingPeriod,
                accountBalanceService.GetAccountBalancesByAccountingPeriod(account, previousAccountingPeriod).EndingBalance.FundBalances);
        }
    }
}