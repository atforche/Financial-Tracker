using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Accounts
/// </summary>
public class AccountService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    TransactionDispatcherService transactionDispatcherService)
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
            if (!transactionDispatcherService.TryDelete(initialTransaction, account.Id, out IEnumerable<Exception> transactionExceptions))
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

        CreateTransactionRequest createRequest = !account.Type.IsTracked()
            ? new CreateAccountTransactionRequest
            {
                AccountingPeriodId = request.AccountingPeriod.Id,
                TransactionDate = request.AddDate,
                Location = "Initial",
                Description = "Initial Balance",
                Amount = request.InitialBalance,
                DebitAccount = account.Type.IsDebt() ? account : null,
                DebitPostedDate = account.Type.IsDebt() ? request.AddDate : null,
                CreditAccount = account.Type.IsDebt() ? null : account,
                CreditPostedDate = account.Type.IsDebt() ? null : request.AddDate,
                GeneratedByAccountId = account.Id
            }
            : account.Type.IsDebt()
            ? new CreateSpendingTransactionRequest
            {
                AccountingPeriodId = request.AccountingPeriod.Id,
                TransactionDate = request.AddDate,
                Location = "Initial",
                Description = "Initial Balance",
                Amount = request.InitialBalance,
                DebitAccount = account,
                DebitPostedDate = request.AddDate,
                CreditAccount = null,
                CreditPostedDate = null,
                FundAssignments = request.InitialFundAssignments,
                IsInitialTransactionForAccount = true
            }
            : new CreateIncomeTransactionRequest
            {
                AccountingPeriodId = request.AccountingPeriod.Id,
                TransactionDate = request.AddDate,
                Location = "Initial",
                Description = "Initial Balance",
                Amount = request.InitialBalance,
                CreditAccount = account,
                CreditPostedDate = request.AddDate,
                DebitAccount = null,
                DebitPostedDate = null,
                FundAssignments = request.InitialFundAssignments,
                IsInitialTransactionForAccount = true
            };
        if (!transactionDispatcherService.TryCreate(createRequest, out Transaction? transaction, out exceptions))
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