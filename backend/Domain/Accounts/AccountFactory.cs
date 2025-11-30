using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts.Exceptions;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Factory for building an <see cref="Account"/>
/// </summary>
public class AccountFactory(IAccountRepository accountRepository, AccountAddedBalanceEventFactory accountAddedBalanceEventFactory)
{
    /// <summary>
    /// Attempts to create a new Account
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="type">Type for the Account</param>
    /// <param name="accountingPeriodId">Accounting Period that the Account is being added to</param>
    /// <param name="addDate">Date the Account is being added</param>
    /// <param name="initialFundAmounts">Initial amounts for each Fund in the Account</param>
    /// <param name="account">The created Account, or null if creation failed</param>
    /// <param name="exceptions">Exceptions encountered during creation</param>
    /// <returns>The newly created Account</returns>
    public bool TryCreate(
        string name,
        AccountType type,
        AccountingPeriodId accountingPeriodId,
        DateOnly addDate,
        IEnumerable<FundAmount> initialFundAmounts,
        [NotNullWhen(true)] out Account? account,
        out IEnumerable<Exception> exceptions)
    {
        account = null;

        if (!ValidateAccountName(name, out exceptions))
        {
            return false;
        }
        account = new Account(name, type);
        if (!accountAddedBalanceEventFactory.TryCreate(new CreateAccountAddedBalanceEventRequest
        {
            AccountingPeriodId = accountingPeriodId,
            EventDate = addDate,
            Account = account,
            FundAmounts = initialFundAmounts.ToList()
        }, out AccountAddedBalanceEvent? accountAddedBalanceEvent, out exceptions))
        {
            account = null;
            return false;
        }
        account.AccountAddedBalanceEvent = accountAddedBalanceEvent;
        return true;
    }

    /// <summary>
    /// Validates the name for this Account
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if name is valid for this Account, false otherwise</returns>
    private bool ValidateAccountName(string name, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(name))
        {
            exceptions = exceptions.Append(new InvalidAccountNameException("Account name cannot be empty"));
        }
        if (accountRepository.TryFindByName(name, out _))
        {
            exceptions = exceptions.Append(new InvalidAccountNameException("Account name must be unique"));
        }
        return !exceptions.Any();
    }
}