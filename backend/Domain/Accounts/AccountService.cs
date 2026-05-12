using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Accounts
/// </summary>
public class AccountService(
    IAccountRepository accountRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Account
    /// </summary>
    public bool TryCreate(
        CreateAccountRequest request,
        [NotNullWhen(true)] out Account? account,
        out IEnumerable<Exception> exceptions)
    {
        account = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        account = new Account(request.Name, request.Type, request.OpeningAccountingPeriod?.Id, request.DateOpened, request.OnboardedBalance);
        accountRepository.Add(account);
        accountingPeriodBalanceService.AddAccount(account);
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

        if (account.IsOnboarded)
        {
            exceptions = [new UnableToDeleteException("Cannot delete an Account that was created during onboarding.")];
            return false;
        }
        if (transactionRepository.DoAnyTransactionsExistForAccount(account))
        {
            exceptions = [new UnableToDeleteException("Cannot delete an Account that has Transactions.")];
            return false;
        }
        accountingPeriodBalanceService.DeleteAccount(account);
        accountRepository.Delete(account);
        return true;
    }

    /// <summary>
    /// Validates the name for this Account
    /// </summary>
    private bool ValidateAccountName(string name, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Account name cannot be empty"));
        }
        if (accountRepository.TryGetByName(name, out _))
        {
            exceptions = exceptions.Append(new InvalidNameException("Account name must be unique"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to create an Account
    /// </summary>
    private bool ValidateCreate(CreateAccountRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateAccountName(request.Name, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (request.OpeningAccountingPeriod != null != (request.DateOpened != null))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Both opening accounting period and date opened must be provided together."));
        }
        if (request.OpeningAccountingPeriod != null && request.OnboardedBalance != null)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Onboarded balance cannot be provided if opening accounting period is provided."));
        }
        if (request.OpeningAccountingPeriod != null && !request.OpeningAccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (request.OpeningAccountingPeriod != null && request.DateOpened != null && !request.OpeningAccountingPeriod.IsDateInPeriod(request.DateOpened.Value))
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided date opened is not within the provided accounting period."));
        }
        return !exceptions.Any();
    }
}