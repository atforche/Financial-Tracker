using System.Diagnostics.CodeAnalysis;
using Domain.Accounts.Exceptions;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Accounts
/// </summary>
public class AccountService(IAccountRepository accountRepository, AccountAddedBalanceEventFactory accountAddedBalanceEventFactory)
{
    /// <summary>
    /// Attempts to create a new Account
    /// </summary>
    /// <param name="request">Request to create the Account</param>
    /// <param name="account">The created Account, or null if creation failed</param>
    /// <param name="exceptions">Exceptions encountered during creation</param>
    /// <returns>The newly created Account</returns>
    public bool TryCreate(CreateAccountRequest request, [NotNullWhen(true)] out Account? account, out IEnumerable<Exception> exceptions)
    {
        account = null;

        if (!ValidateAccountName(request.Name, out exceptions))
        {
            return false;
        }
        account = new Account(request.Name, request.Type);
        if (!accountAddedBalanceEventFactory.TryCreate(new CreateAccountAddedBalanceEventRequest
        {
            AccountingPeriodId = request.AccountingPeriodId,
            EventDate = request.AddDate,
            Account = account,
            FundAmounts = request.InitialFundAmounts.ToList()
        }, out AccountAddedBalanceEvent? accountAddedBalanceEvent, out exceptions))
        {
            account = null;
            return false;
        }
        account.AccountAddedBalanceEvent = accountAddedBalanceEvent;
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Account
    /// </summary>
    /// <param name="account">Account to be updated</param>
    /// <param name="name">New name for the Account</param>
    /// <param name="exceptions">List of exceptions encountered during update</param>
    /// <returns>True if update was successful, false otherwise</returns>
    public bool TryUpdate(Account account, string name, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateAccountName(name, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
            return false;
        }
        account.Name = name;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Account
    /// </summary>
    /// <param name="account">Account to be deleted</param>
    /// <param name="exceptions">List of exceptions encountered during deletion</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    public bool TryDelete(Account account, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        accountRepository.Delete(account);
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