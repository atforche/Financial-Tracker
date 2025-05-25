using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.Actions;

/// <summary>
/// Action class that adds an Account
/// </summary>
/// <param name="accountRepository">Account Repository</param>
/// <param name="balanceEventRepository">Balance Event Repository</param>
/// <param name="balanceEventDateValidator">Balance Event Date Validator</param>
public class AddAccountAction(
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    BalanceEventDateValidator balanceEventDateValidator)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="type">Type for the Account</param>
    /// <param name="accountingPeriod">Accounting Period for the Account Added Balance Event for this Account</param>
    /// <param name="date">Date for the Account Added Balance Event for this Account</param>
    /// <param name="fundAmounts">Fund Amounts for the Account Added Balance Event for this Account</param>
    /// <returns>The Account that was added</returns>
    public Account Run(
        string name,
        AccountType type,
        AccountingPeriod accountingPeriod,
        DateOnly date,
        IEnumerable<FundAmount> fundAmounts)
    {
        if (!IsValid(name, accountingPeriod, date, out Exception? exception))
        {
            throw exception;
        }
        var newAccount = new Account(name,
            type,
            accountingPeriod,
            date,
            balanceEventRepository.GetHighestEventSequenceOnDate(date) + 1,
            fundAmounts);
        accountingPeriod.AddAccountAddedBalanceEvent(newAccount.AccountAddedBalanceEvent);
        return newAccount;
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="accountingPeriod">First Accounting Period for this Account</param>
    /// <param name="date">First Date for this Account</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private bool IsValid(string name, AccountingPeriod accountingPeriod, DateOnly date, [NotNullWhen(false)] out Exception? exception)
    {
        if (!balanceEventDateValidator.Validate(accountingPeriod, [], date, out exception))
        {
            return false;
        }
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException();
        }
        if (accountRepository.FindByNameOrNull(name) != null)
        {
            exception = new InvalidOperationException();
        }
        if (!accountingPeriod.IsOpen)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}