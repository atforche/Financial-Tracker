using System.Diagnostics.CodeAnalysis;
using Domain.Accounts.Exceptions;
using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Accounts
/// </summary>
public class AccountService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    TransactionService transactionService)
{
    /// <summary>
    /// Attempts to create a new Account
    /// </summary>
    public bool TryCreate(
        CreateAccountRequest request,
        [NotNullWhen(true)] out Account? account,
        out Transaction? initialTransaction,
        out IEnumerable<Exception> exceptions)
    {
        account = null;
        initialTransaction = null;
        exceptions = [];

        if (!ValidateAccountName(request.Name, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (!request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException());
        }
        if (!request.AccountingPeriod.IsDateInPeriod(request.AddDate))
        {
            exceptions = exceptions.Append(new InvalidAddDateException());
        }
        if (exceptions.Any())
        {
            return false;
        }
        account = new Account(request.Name, request.Type, request.AccountingPeriod.Id, request.AddDate);
        if (request.InitialFundAmounts.Any())
        {
            if (!transactionService.TryCreate(new CreateTransactionRequest
            {
                AccountingPeriod = request.AccountingPeriod,
                Date = request.AddDate,
                Location = "Initial",
                Description = "Initial Balance",
                DebitAccount = null,
                CreditAccount = new CreateTransactionAccountRequest
                {
                    Account = account,
                    FundAmounts = request.InitialFundAmounts
                },
                IsInitialTransactionForAccount = true
            }, out initialTransaction, out IEnumerable<Exception> transactionExceptions))
            {
                exceptions = exceptions.Concat(transactionExceptions);
                account = null;
                return false;
            }
            if (!transactionService.TryPost(initialTransaction, account.Id, request.AddDate, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
                account = null;
                return false;
            }
            account.InitialTransaction = initialTransaction.Id;
        }
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

        if (transactionRepository.DoAnyTransactionsExistForAccount(account))
        {
            exceptions = [new UnableToDeleteException("Cannot delete an Account that has Transactions.")];
            return false;
        }
        if (account.InitialTransaction != null)
        {
            Transaction initialTransaction = transactionRepository.GetById(account.InitialTransaction);
            if (!transactionService.TryDelete(initialTransaction, account.Id, out IEnumerable<Exception> transactionExceptions))
            {
                exceptions = exceptions.Concat(transactionExceptions);
                return false;
            }
        }
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
            exceptions = exceptions.Append(new InvalidNameException("Account name cannot be empty"));
        }
        if (accountRepository.TryGetByName(name, out _))
        {
            exceptions = exceptions.Append(new InvalidNameException("Account name must be unique"));
        }
        return !exceptions.Any();
    }
}