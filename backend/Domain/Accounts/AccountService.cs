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
        if (request.Type.IsTracked())
        {
            Fund? unassignedFund = fundRepository.GetUnassignedFund();
            if (unassignedFund == null)
            {
                if (!fundService.TryOnboardUnassignedFund(0, out Fund? newUnassignedFund, out IEnumerable<Exception> unassignedFundExceptions))
                {
                    exceptions = exceptions.Concat(unassignedFundExceptions);
                    return false;
                }
                unassignedFund = newUnassignedFund;
            }
            decimal changeInUnassignedBalance = request.Type.IsDebt() ? -request.OnboardedBalance : request.OnboardedBalance;
            unassignedFund.OnboardedBalance += changeInUnassignedBalance;
        }
        account = new Account(request.Name, request.Type, request.OnboardedBalance);
        accountRepository.Add(account);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Account
    /// </summary>
    public bool TryUpdate(Account account, string name, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateAccountName(name, account, out exceptions))
        {
            return false;
        }
        account.Name = name;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Account
    /// </summary>
    public bool TryDelete(Account account, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(account, out exceptions))
        {
            return false;
        }
        if (account.OnboardedBalance != null)
        {
            Fund unassignedFund = fundRepository.GetUnassignedFund() ?? throw new InvalidOperationException();
            decimal changeInUnassignedBalance = account.Type.IsDebt() ? -account.OnboardedBalance.Value : account.OnboardedBalance.Value;
            unassignedFund.OnboardedBalance -= changeInUnassignedBalance;
        }
        accountingPeriodBalanceService.DeleteAccount(account);
        accountRepository.Delete(account);
        return true;
    }

    /// <summary>
    /// Validates the name for this Account
    /// </summary>
    private bool ValidateAccountName(string name, Account? existingAccount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrWhiteSpace(name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Account name cannot be empty"));
        }
        if (accountRepository.TryGetByName(name, out Account? accountWithName) && accountWithName != existingAccount)
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

        if (!ValidateAccountName(request.Name, null, out IEnumerable<Exception> nameExceptions))
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
        if (!Enum.IsDefined(request.Type))
        {
            exceptions = exceptions.Append(new InvalidAccountTypeException());
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to onboard an Account.
    /// </summary>
    private bool ValidateOnboard(OnboardAccountRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateAccountName(request.Name, null, out IEnumerable<Exception> nameExceptions))
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
        if (!Enum.IsDefined(request.Type))
        {
            exceptions = exceptions.Append(new InvalidAccountTypeException());
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

    /// <summary>
    /// Validates a request to delete an Account.
    /// </summary>
    private bool ValidateDelete(Account account, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (account.IsOnboarded && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Cannot delete an onboarded Account."));
        }
        if (transactionRepository.DoAnyTransactionsExistForAccount(account))
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Cannot delete an Account that has Transactions."));
        }
        if (account.OnboardedBalance != null && !account.Type.IsDebt())
        {
            Fund unassignedFund = fundRepository.GetUnassignedFund() ?? throw new InvalidOperationException();
            if (unassignedFund.OnboardedBalance - account.OnboardedBalance < 0)
            {
                exceptions = exceptions.Append(new InvalidFundException("Deleting this Account would cause the unassigned fund balance to go negative."));
            }
        }
        return !exceptions.Any();
    }
}