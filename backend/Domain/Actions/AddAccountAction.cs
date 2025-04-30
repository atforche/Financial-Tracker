using System.Diagnostics.CodeAnalysis;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Actions;

/// <summary>
/// Action class that adds an Account
/// </summary>
/// <param name="accountRepository">Account Repository</param>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
public class AddAccountAction(IAccountRepository accountRepository, IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="type">Type for the Account</param>
    /// <param name="startingFundBalances">Starting Fund Balances for the Account</param>
    /// <returns>The Account that was added</returns>
    public Account Run(string name, AccountType type, IEnumerable<FundAmount> startingFundBalances)
    {
        if (!IsValid(name, out Exception? exception))
        {
            throw exception;
        }
        var newAccount = new Account(name, type);
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindOpenPeriods().First();
        newAccount.AddAccountBalanceCheckpoint(accountingPeriod, startingFundBalances);
        return newAccount;
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private bool IsValid(string name, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (accountRepository.FindByNameOrNull(name) != null)
        {
            exception = new InvalidOperationException();
        }
        if (accountingPeriodRepository.FindOpenPeriods().Count == 0)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}