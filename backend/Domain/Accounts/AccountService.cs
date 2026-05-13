using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Accounts
/// </summary>
public class AccountService(
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundService fundService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IFundRepository fundRepository,
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
        account = new Account(request.Name, request.Type, request.OpeningAccountingPeriod.Id, request.DateOpened);
        accountRepository.Add(account);
        accountingPeriodBalanceService.AddAccount(account);
        return true;
    }

    /// <summary>
    /// Attempts to onboard a new Account.
    /// </summary>
    public bool TryOnboard(
        OnboardAccountRequest request,
        [NotNullWhen(true)] out Account? account,
        out IEnumerable<Exception> exceptions)
    {
        account = null;

        if (!ValidateOnboard(request, out exceptions))
        {
            return false;
        }
        account = new Account(request.Name, request.Type, request.OnboardedBalance);
        accountRepository.Add(account);
        if (request.Type.IsTracked())
        {
            Fund? unassignedFund = fundRepository.GetUnassignedFund();
            if (unassignedFund == null)
            {
                if (!fundService.TryOnboard(new OnboardFundRequest
                {
                    Name = Fund.UnassignedFundName,
                    Description = Fund.UnassignedFundDescription,
                    OnboardedBalance = 0,
                }, out Fund? newUnassignedFund, out IEnumerable<Exception> unassignedFundExceptions))
                {
                    exceptions = exceptions.Concat(unassignedFundExceptions);
                    return false;
                }
                fundRepository.Add(newUnassignedFund);
                unassignedFund = newUnassignedFund;
            }
            decimal changeInUnassignedBalance = request.Type.IsDebt() ? -request.OnboardedBalance : request.OnboardedBalance;
            unassignedFund.OnboardedBalance += changeInUnassignedBalance;
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

        if (account.IsOnboarded)
        {
            exceptions = [new UnableToDeleteException("Cannot delete an onboarded Account.")];
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
        if (!request.OpeningAccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (!request.OpeningAccountingPeriod.IsDateInPeriod(request.DateOpened))
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided date opened is not within the provided accounting period."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to onboard an Account.
    /// </summary>
    private bool ValidateOnboard(OnboardAccountRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateAccountName(request.Name, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (accountingPeriodRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("Accounts can only be onboarded before any Accounting Periods have been created."));
        }
        if (request.OnboardedBalance < 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("Account balance cannot be negative."));
        }
        if (request.Type.IsTracked())
        {
            decimal startingUnassignedBalance = fundRepository.GetUnassignedFund()?.OnboardedBalance ?? 0;
            decimal updatedUnassignedBalance = startingUnassignedBalance + (request.Type.IsDebt() ? -request.OnboardedBalance : request.OnboardedBalance);
            if (updatedUnassignedBalance < 0)
            {
                exceptions = exceptions.Append(new InvalidFundException("Onboarding this Account would cause the unassigned fund balance to go negative."));
            }
        }

        return !exceptions.Any();
    }
}