using System.Diagnostics.CodeAnalysis;
using Domain.AccountGoals;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;
using Domain.Validation;

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
    ITransactionRepository transactionRepository,
    AccountGoalService accountGoalService)
{
    /// <summary>
    /// Attempts to create a new Account
    /// </summary>
    public bool TryCreate(
        CreateAccountRequest request,
        [NotNullWhen(true)] out Account? account,
        out IEnumerable<ValidationError> errors)
    {
        account = null;

        if (!ValidateCreate(request, out errors))
        {
            return false;
        }
        account = new Account(
            request.Name,
            NormalizeFinancialInstitution(request.FinancialInstitution),
            request.Type,
            request.OpeningAccountingPeriod.Id,
            request.DateOpened);
        accountRepository.Add(account);
        accountGoalService.CreateForAccount(account);
        accountingPeriodBalanceService.AddAccount(account);
        return true;
    }

    /// <summary>
    /// Attempts to onboard a new Account.
    /// </summary>
    public bool TryOnboard(
        OnboardAccountRequest request,
        [NotNullWhen(true)] out Account? account,
        out IEnumerable<ValidationError> errors)
    {
        account = null;

        if (!ValidateOnboard(request, out errors))
        {
            return false;
        }
        if (request.Type.IsTracked())
        {
            Fund? unassignedFund = fundRepository.GetUnassignedFund();
            if (unassignedFund == null)
            {
                if (!fundService.TryOnboardUnassignedFund(0, out Fund? newUnassignedFund, out IEnumerable<ValidationError> unassignedFundErrors))
                {
                    errors = errors.Concat(unassignedFundErrors);
                    return false;
                }
                unassignedFund = newUnassignedFund;
            }
            decimal changeInUnassignedBalance = request.Type.IsDebt() ? -request.OnboardedBalance : request.OnboardedBalance;
            unassignedFund.OnboardedBalance += changeInUnassignedBalance;
        }
        account = new Account(
            request.Name,
            NormalizeFinancialInstitution(request.FinancialInstitution),
            request.Type,
            request.OnboardedBalance);
        accountRepository.Add(account);
        accountGoalService.CreateForAccount(account);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Account
    /// </summary>
    public bool TryUpdate(Account account, UpdateAccountRequest request, out IEnumerable<ValidationError> errors)
    {
        if (!ValidateAccountName(
                request.Name,
                new ValidationErrorPath(nameof(UpdateAccountRequest.Name)),
                account,
                out errors))
        {
            return false;
        }
        account.Name = request.Name;
        account.FinancialInstitution = NormalizeFinancialInstitution(request.FinancialInstitution);
        return true;
    }

    /// <summary>
    /// Normalizes an optional financial institution value before it is persisted.
    /// </summary>
    private static string? NormalizeFinancialInstitution(string? financialInstitution) =>
        string.IsNullOrWhiteSpace(financialInstitution) ? null : financialInstitution.Trim();

    /// <summary>
    /// Attempts to delete an existing Account
    /// </summary>
    public bool TryDelete(Account account, out IEnumerable<ValidationError> errors)
    {
        if (!ValidateDelete(account, out errors))
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
        accountGoalService.DeleteForAccount(account);
        accountRepository.Delete(account);
        return true;
    }

    /// <summary>
    /// Validates the name for this Account
    /// </summary>
    private bool ValidateAccountName(
        string name,
        ValidationErrorPath namePath,
        Account? existingAccount,
        out IEnumerable<ValidationError> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(name))
        {
            errors = errors.Append(new ValidationError(namePath, "Account name cannot be empty"));
        }
        if (accountRepository.TryGetByName(name, out Account? accountWithName) && accountWithName != existingAccount)
        {
            errors = errors.Append(new ValidationError(namePath, "Account name must be unique"));
        }
        return !errors.Any();
    }

    /// <summary>
    /// Validates a request to create an Account
    /// </summary>
    private bool ValidateCreate(CreateAccountRequest request, out IEnumerable<ValidationError> errors)
    {
        errors = [];

        if (!ValidateAccountName(
                request.Name,
                new ValidationErrorPath(nameof(CreateAccountRequest.Name)),
                null,
                out IEnumerable<ValidationError> nameErrors))
        {
            errors = errors.Concat(nameErrors);
        }
        if (!request.OpeningAccountingPeriod.IsOpen)
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountRequest.OpeningAccountingPeriod)),
                "The provided accounting period is closed."));
        }
        if (!request.OpeningAccountingPeriod.IsDateInPeriod(request.DateOpened))
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountRequest.DateOpened)),
                "The provided date opened is not within the provided accounting period."));
        }
        if (!Enum.IsDefined(request.Type))
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountRequest.Type)),
                "The provided account type is invalid."));
        }
        return !errors.Any();
    }

    /// <summary>
    /// Validates a request to onboard an Account.
    /// </summary>
    private bool ValidateOnboard(OnboardAccountRequest request, out IEnumerable<ValidationError> errors)
    {
        errors = [];

        if (!ValidateAccountName(
                request.Name,
                new ValidationErrorPath(nameof(OnboardAccountRequest.Name)),
                null,
                out IEnumerable<ValidationError> nameErrors))
        {
            errors = errors.Concat(nameErrors);
        }
        if (accountingPeriodRepository.GetAll().Count > 0)
        {
            errors = errors.Append(new ValidationError(ValidationErrorPath.Empty, "Accounts can only be onboarded before any Accounting Periods have been created."));
        }
        if (request.OnboardedBalance < 0)
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(OnboardAccountRequest.OnboardedBalance)),
                "Account balance cannot be negative."));
        }
        bool hasValidAccountType = Enum.IsDefined(request.Type);
        if (!hasValidAccountType)
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(OnboardAccountRequest.Type)),
                "The provided account type is invalid."));
        }
        if (hasValidAccountType && request.Type.IsTracked())
        {
            decimal startingUnassignedBalance = fundRepository.GetUnassignedFund()?.OnboardedBalance ?? 0;
            decimal updatedUnassignedBalance = startingUnassignedBalance + (request.Type.IsDebt() ? -request.OnboardedBalance : request.OnboardedBalance);
            if (updatedUnassignedBalance < 0)
            {
                errors = errors.Append(new ValidationError(ValidationErrorPath.Empty, "Onboarding this Account would cause the unassigned fund balance to go negative."));
            }
        }
        return !errors.Any();
    }

    /// <summary>
    /// Validates a request to delete an Account.
    /// </summary>
    private bool ValidateDelete(Account account, out IEnumerable<ValidationError> errors)
    {
        errors = [];

        if (account.IsOnboarded && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            errors = errors.Append(new ValidationError(ValidationErrorPath.Empty, "Cannot delete an onboarded Account."));
        }
        if (transactionRepository.DoAnyTransactionsExistForAccount(account))
        {
            errors = errors.Append(new ValidationError(ValidationErrorPath.Empty, "Cannot delete an Account that has Transactions."));
        }
        if (account.OnboardedBalance != null && !account.Type.IsDebt())
        {
            Fund unassignedFund = fundRepository.GetUnassignedFund() ?? throw new InvalidOperationException();
            if (unassignedFund.OnboardedBalance - account.OnboardedBalance < 0)
            {
                errors = errors.Append(new ValidationError(
                    ValidationErrorPath.Empty,
                    "Deleting this Account would cause the unassigned fund balance to go negative."));
            }
        }
        return !errors.Any();
    }
}
