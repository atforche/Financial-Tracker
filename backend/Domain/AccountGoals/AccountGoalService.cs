using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Validation;

namespace Domain.AccountGoals;

/// <summary>
/// Service for creating and updating Account Goals.
/// </summary>
public sealed class AccountGoalService(
    IAccountGoalRepository accountGoalRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository)
{
    /// <summary>
    /// Attempts to create an Account Goal.
    /// </summary>
    public bool TryCreate(
        CreateAccountGoalRequest request,
        [NotNullWhen(true)] out AccountGoal? accountGoal,
        out IEnumerable<ValidationError> errors)
    {
        accountGoal = null;
        errors = Validate(request);
        if (accountGoalRepository.GetByAccountAndAccountingPeriod(request.Account.Id, request.AccountingPeriod?.Id) != null)
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountGoalRequest.Account)),
                "An Account Goal already exists for this Account and Accounting Period."));
        }
        if (errors.Any())
        {
            return false;
        }

        accountGoal = new AccountGoal(
            request.Account,
            request.AccountingPeriod,
            request.MinimumEndingBalance,
            request.MaximumEndingBalance);
        if (!accountGoalRepository.TryAdd(accountGoal))
        {
            accountGoal = null;
            errors = [new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountGoalRequest.Account)),
                "An Account Goal already exists for this Account and Accounting Period.")];
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to retrieve an Account Goal for update.
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountGoal? accountGoal) =>
        accountGoalRepository.TryGetById(id, out accountGoal);

    /// <summary>
    /// Updates an Account Goal's configuration.
    /// </summary>
    public static bool TryUpdate(
        AccountGoal accountGoal,
        UpdateAccountGoalRequest request,
        out IEnumerable<ValidationError> errors)
    {
        errors = Validate(request.MinimumEndingBalance, request.MaximumEndingBalance);
        if (accountGoal.AccountingPeriod is { IsOpen: false })
        {
            errors = errors.Append(new ValidationError(
                ValidationErrorPath.Empty,
                "An Account Goal for a closed Accounting Period cannot be changed."));
        }
        if (errors.Any())
        {
            return false;
        }

        accountGoal.Update(request.MinimumEndingBalance, request.MaximumEndingBalance);
        return true;
    }

    /// <summary>
    /// Creates a new Account Goal with copied configuration.
    /// </summary>
    public AccountGoal Copy(AccountGoal source, Account account, AccountingPeriod? accountingPeriod) =>
        CreateOrThrow(new CreateAccountGoalRequest
        {
            Account = account,
            AccountingPeriod = accountingPeriod,
            MinimumEndingBalance = source.MinimumEndingBalance,
            MaximumEndingBalance = source.MaximumEndingBalance,
        });

    /// <summary>
    /// Creates default Account Goals for an Account across its applicable Accounting Periods.
    /// </summary>
    public void CreateForAccount(Account account)
    {
        if (account.Type != AccountType.Standard)
        {
            return;
        }
        if (account.IsOnboarded)
        {
            _ = CreateOrThrow(new CreateAccountGoalRequest { Account = account });
            return;
        }

        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(account.OpeningAccountingPeriodId!);
        while (accountingPeriod != null)
        {
            _ = CreateOrThrow(new CreateAccountGoalRequest
            {
                Account = account,
                AccountingPeriod = accountingPeriod,
            });
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Copies Account Goals from the previous Accounting Period, or onboarded Account Goals for the first Accounting Period.
    /// </summary>
    public void CopyToAccountingPeriod(AccountingPeriod? previousAccountingPeriod, AccountingPeriod accountingPeriod)
    {
        foreach (AccountGoal existingGoal in accountGoalRepository.GetAllByAccountingPeriod(previousAccountingPeriod?.Id))
        {
            _ = Copy(existingGoal, existingGoal.Account, accountingPeriod);
        }

        IReadOnlyCollection<Account> standardAccounts = accountRepository.GetAll()
            .Where(account => account.Type == AccountType.Standard)
            .ToList();
        IReadOnlyCollection<AccountGoal> copiedGoals = accountGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id);
        bool hasExactlyOneGoalPerAccount = standardAccounts.All(account =>
            copiedGoals.Count(goal => goal.Account.Id == account.Id) == 1)
            && copiedGoals.All(goal => goal.Account.Type == AccountType.Standard
                && standardAccounts.Any(account => account.Id == goal.Account.Id));
        if (!hasExactlyOneGoalPerAccount)
        {
            throw new InvalidOperationException(
                "Every standard Account must have exactly one Account Goal for the Accounting Period.");
        }
    }

    /// <summary>
    /// Deletes all Account Goals associated with an Account.
    /// </summary>
    public void DeleteForAccount(Account account)
    {
        foreach (AccountGoal accountGoal in accountGoalRepository.GetAllByAccount(account.Id))
        {
            accountGoalRepository.Delete(accountGoal);
        }
    }

    /// <summary>
    /// Deletes all Account Goals associated with an Accounting Period.
    /// </summary>
    public void DeleteForAccountingPeriod(AccountingPeriod accountingPeriod)
    {
        foreach (AccountGoal accountGoal in accountGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id))
        {
            accountGoalRepository.Delete(accountGoal);
        }
    }

    /// <summary>
    /// Calculates progress for an Account Goal in the provided Accounting Period.
    /// </summary>
    public bool TryGetProgress(
        AccountGoal accountGoal,
        AccountingPeriod accountingPeriod,
        [NotNullWhen(true)] out AccountGoalProgress? progress,
        out IEnumerable<ValidationError> errors)
    {
        progress = null;
        errors = [];
        if (accountGoal.AccountingPeriod?.Id != accountingPeriod.Id)
        {
            errors = [new ValidationError(
                new ValidationErrorPath(nameof(accountingPeriod)),
                "The Account Goal does not apply to the provided Accounting Period.")];
            return false;
        }
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(accountingPeriod.Id);
        AccountingPeriodAccountBalanceHistory? accountBalance = balanceHistory.AccountBalances
            .SingleOrDefault(balance => balance.Account.Id == accountGoal.Account.Id);
        if (accountBalance == null)
        {
            errors = [new ValidationError(
                new ValidationErrorPath(nameof(accountingPeriod)),
                "The Account Goal does not apply to the provided Accounting Period.")];
            return false;
        }

        progress = AccountGoalProgressService.Calculate(
            accountBalance.ClosingBalance,
            accountGoal.MinimumEndingBalance,
            accountGoal.MaximumEndingBalance);
        return true;
    }

    /// <summary>
    /// Calculates progress for Account Goals in the same Accounting Period using one balance-history lookup.
    /// </summary>
    public IReadOnlyDictionary<AccountGoalId, AccountGoalProgress> GetProgresses(
        IReadOnlyCollection<AccountGoal> accountGoals,
        AccountingPeriod accountingPeriod)
    {
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(accountingPeriod.Id);
        var accountBalances = balanceHistory.AccountBalances.ToDictionary(balance => balance.Account.Id);
        var results = new Dictionary<AccountGoalId, AccountGoalProgress>();
        foreach (AccountGoal accountGoal in accountGoals)
        {
            if (accountGoal.AccountingPeriod?.Id != accountingPeriod.Id
                || !accountBalances.TryGetValue(accountGoal.Account.Id, out AccountingPeriodAccountBalanceHistory? accountBalance))
            {
                continue;
            }
            results.Add(accountGoal.Id, AccountGoalProgressService.Calculate(
                accountBalance.ClosingBalance,
                accountGoal.MinimumEndingBalance,
                accountGoal.MaximumEndingBalance));
        }
        return results;
    }

    /// <summary>
    /// Creates an Account Goal and throws if its configuration violates an invariant.
    /// </summary>
    private AccountGoal CreateOrThrow(CreateAccountGoalRequest request)
    {
        if (!TryCreate(request, out AccountGoal? accountGoal, out IEnumerable<ValidationError> errors))
        {
            throw new InvalidOperationException(string.Join(" ", errors.Select(error => error.Message)));
        }
        return accountGoal;
    }

    /// <summary>
    /// Validates an Account Goal creation request.
    /// </summary>
    private IEnumerable<ValidationError> Validate(CreateAccountGoalRequest request)
    {
        IEnumerable<ValidationError> errors = Validate(request.MinimumEndingBalance, request.MaximumEndingBalance);
        if (request.Account.Type != AccountType.Standard)
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountGoalRequest.Account)),
                "Account Goals can only be associated with standard Accounts."));
        }
        if (request.AccountingPeriod == null)
        {
            if (!request.Account.IsOnboarded)
            {
                errors = errors.Append(new ValidationError(
                    new ValidationErrorPath(nameof(CreateAccountGoalRequest.AccountingPeriod)),
                    "Only onboarded Accounts can have an Account Goal without an Accounting Period."));
            }
        }
        else
        {
            if (!request.AccountingPeriod.IsOpen)
            {
                errors = errors.Append(new ValidationError(
                    new ValidationErrorPath(nameof(CreateAccountGoalRequest.AccountingPeriod)),
                    "The provided Accounting Period is closed."));
            }
            if (request.Account.OpeningAccountingPeriodId is AccountingPeriodId openingPeriodId)
            {
                AccountingPeriod? openingPeriod = accountingPeriodRepository.GetAll()
                    .SingleOrDefault(period => period.Id == openingPeriodId);
                if (openingPeriod == null)
                {
                    errors = errors.Append(new ValidationError(
                        new ValidationErrorPath(nameof(CreateAccountGoalRequest.Account)),
                        "The Account's opening Accounting Period does not exist."));
                }
                else if (request.AccountingPeriod.PeriodStartDate < openingPeriod.PeriodStartDate)
                {
                    errors = errors.Append(new ValidationError(
                        new ValidationErrorPath(nameof(CreateAccountGoalRequest.AccountingPeriod)),
                        "An Account Goal cannot predate the Account's opening Accounting Period."));
                }
            }
        }
        return errors;
    }

    /// <summary>
    /// Validates Account Goal bounds.
    /// </summary>
    private static IEnumerable<ValidationError> Validate(decimal? minimumEndingBalance, decimal? maximumEndingBalance)
    {
        IEnumerable<ValidationError> errors = [];
        errors = errors
            .Concat(ValidateNonnegative(minimumEndingBalance, nameof(UpdateAccountGoalRequest.MinimumEndingBalance)))
            .Concat(ValidateNonnegative(maximumEndingBalance, nameof(UpdateAccountGoalRequest.MaximumEndingBalance)));
        if (minimumEndingBalance > maximumEndingBalance)
        {
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateAccountGoalRequest.MinimumEndingBalance)),
                "Minimum ending balance must be less than or equal to maximum ending balance."));
            errors = errors.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateAccountGoalRequest.MaximumEndingBalance)),
                "Maximum ending balance must be greater than or equal to minimum ending balance."));
        }
        return errors;
    }

    /// <summary>
    /// Validates an optional nonnegative Account Goal bound.
    /// </summary>
    private static IEnumerable<ValidationError> ValidateNonnegative(decimal? value, string propertyName) =>
        value < 0
            ? [new ValidationError(
                new ValidationErrorPath(propertyName),
                "Account Goal bounds must be greater than or equal to zero.")]
            : [];
}
