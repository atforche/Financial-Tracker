using System.Diagnostics.CodeAnalysis;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;

namespace Domain.Actions;

/// <summary>
/// Action class that closes an Accounting Period
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
/// <param name="accountRepository">Account Repository</param>
/// <param name="accountBalanceService">Account Balance Service</param>
public class CloseAccountingPeriodAction(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IAccountBalanceService accountBalanceService)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to close</param>
    public void Run(AccountingPeriod accountingPeriod)
    {
        if (!IsValid(accountingPeriod, out Exception? exception))
        {
            throw exception;
        }
        accountingPeriod.IsOpen = false;
        AddAccountBalanceCheckpoints(accountingPeriod);
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to close</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private bool IsValid(AccountingPeriod accountingPeriod, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (!accountingPeriod.IsOpen)
        {
            exception = new InvalidOperationException();
        }
        // Validate that there are no other earlier open Accounting Periods
        if (accountingPeriodRepository.FindOpenPeriods().Any(openPeriod => openPeriod.PeriodStartDate < accountingPeriod.PeriodStartDate))
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that there are no pending balance changes in this Accounting Period
        if (accountRepository.FindAll()
                .Select(account => accountBalanceService.GetAccountBalancesByAccountingPeriod(account, accountingPeriod))
                .Any(balance => balance.EndingBalance.PendingFundBalanceChanges.Count != 0))
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Adds Account Balance Checkpoints for the future Accounting Period if necessary
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to close</param>
    private void AddAccountBalanceCheckpoints(AccountingPeriod accountingPeriod)
    {
        AccountingPeriod? futureAccountingPeriod =
            accountingPeriodRepository.FindByDateOrNull(accountingPeriod.PeriodStartDate.AddMonths(1));
        if (futureAccountingPeriod == null)
        {
            return;
        }
        foreach (Account account in accountRepository.FindAll())
        {
            account.AddAccountBalanceCheckpoint(futureAccountingPeriod,
                accountBalanceService.GetAccountBalancesByAccountingPeriod(account, accountingPeriod).EndingBalance.FundBalances);
        }
    }
}