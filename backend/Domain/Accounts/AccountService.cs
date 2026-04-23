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
    ITransactionRepository transactionRepository,
    TransactionService transactionService,
    AccountingPeriodBalanceService accountingPeriodBalanceService)
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
        account = new Account(request.Name, request.Type, request.AccountingPeriod.Id, request.AddDate);
        accountRepository.Add(account);
        accountingPeriodBalanceService.AddAccount(account);
        if (!AddInitialAccountTransaction(request, account, out exceptions))
        {
            return false;
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
        accountingPeriodBalanceService.DeleteAccount(account);
        accountRepository.Delete(account);
        return true;
    }

    /// <summary>
    /// Adds the initial transaction for an Account if the initial balance is greater than 0
    /// </summary>
    private bool AddInitialAccountTransaction(CreateAccountRequest request, Account account, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.InitialBalance <= 0)
        {
            return true;
        }

        CreateTransactionRequest createTransactionRequest = !account.Type.IsDebt()
            ? new CreateIncomeTransactionRequest
            {
                AccountingPeriodId = request.AccountingPeriod.Id,
                TransactionDate = request.AddDate,
                Location = "Initial",
                Description = "Initial Balance",
                Account = account,
                Amount = request.InitialBalance,
                FundAssignments = request.InitialFundAssignments,
                IsInitialTransactionForAccount = true
            }
            : new CreateSpendingTransactionRequest
            {
                AccountingPeriodId = request.AccountingPeriod.Id,
                TransactionDate = request.AddDate,
                Location = "Initial",
                Description = "Initial Balance",
                Account = account,
                Amount = request.InitialBalance,
                FundAssignments = request.InitialFundAssignments,
                IsInitialTransactionForAccount = true
            };
        if (!transactionService.TryCreate(createTransactionRequest, out Transaction? transaction, out exceptions))
        {
            return false;
        }
        if (!transactionService.TryPost(transaction, account.Id, request.AddDate, out exceptions))
        {
            return false;
        }
        account.InitialTransaction = transaction.Id;
        transactionRepository.Add(transaction);
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
        if (!request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (!request.AccountingPeriod.IsDateInPeriod(request.AddDate))
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided add date is not within the provided accounting period."));
        }
        if (!request.Type.IsTracked() && request.InitialFundAssignments.Count > 0)
        {
            exceptions = exceptions.Append(new InvalidAccountTypeException("Cannot assign funds to an untracked account."));
        }
        return !exceptions.Any();
    }
}