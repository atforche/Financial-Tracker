using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Income;
using Domain.Payroll;
using Domain.Transactions;
using Domain.Validation;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Periods
/// </summary>
public class AccountingPeriodService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundGoalService fundGoalService,
    FundService fundService)
{
    /// <summary>
    /// Attempts to create a new Accounting Period
    /// </summary>
    public bool TryCreate(
        CreateAccountingPeriodRequest request,
        [NotNullWhen(true)] out AccountingPeriod? accountingPeriod,
        out IEnumerable<ValidationError> exceptions)
    {
        accountingPeriod = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        AccountingPeriod? previousAccountingPeriod = accountingPeriodRepository.GetLatestAccountingPeriod();
        accountingPeriod = new AccountingPeriod(request.Year, request.Month);
        accountingPeriod.ReplaceExpectedIncomeSources(request.ExpectedIncomeSources.Count > 0
            ? request.ExpectedIncomeSources
            : previousAccountingPeriod?.ExpectedIncomeSources.Select(source => new ExpectedIncomeSourceRequest
            {
                Name = source.Name,
                Income = IncomeBreakdownSnapshot.Create(source.Income),
                ExpectedDates = [],
            }) ?? []);
        fundGoalService.CopyToAccountingPeriod(previousAccountingPeriod, accountingPeriod);
        accountingPeriodBalanceService.AddAccountingPeriod(accountingPeriod);
        if (previousAccountingPeriod == null)
        {
            return TryCreateFirstAccountingPeriod(accountingPeriod, out exceptions);
        }
        exceptions = [];
        return true;
    }

    /// <summary>
    /// Attempts to close an existing Accounting Period
    /// </summary>
    public bool TryClose(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateClose(accountingPeriod, out exceptions))
        {
            return false;
        }
        accountingPeriod.IsOpen = false;
        return true;
    }

    /// <summary>
    /// Attempts to reopen a closed Accounting Period
    /// </summary>
    public bool TryReopen(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateReopen(accountingPeriod, out exceptions))
        {
            return false;
        }
        accountingPeriod.IsOpen = true;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Accounting Period
    /// </summary>
    public bool TryDelete(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateDelete(accountingPeriod, out exceptions))
        {
            return false;
        }
        accountingPeriodBalanceService.DeleteAccountingPeriod(accountingPeriod);
        fundGoalService.DeleteForAccountingPeriod(accountingPeriod);
        if (fundRepository.GetAllFundsAddedInPeriod(accountingPeriod.Id).FirstOrDefault(fund => fund.IsUnassignedFund) is Fund unassignedFund)
        {
            // If the unassigned fund was added in this accounting period, delete it.
            // It will be added again when a new accounting period is created.
            fundRepository.Delete(unassignedFund);
        }
        accountingPeriodRepository.Delete(accountingPeriod);
        return true;
    }

    /// <summary>
    /// Validates creating a new Accounting Period
    /// </summary>
    private bool ValidateCreate(CreateAccountingPeriodRequest request, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (request.Year is < 2000 or > 2100)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountingPeriodRequest.Year)),
                "The provided year must be between 2000 and 2100."));
        }
        if (request.Month is <= 0 or > 12)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountingPeriodRequest.Month)),
                "The provided month must be between 1 and 12."));
        }
        if (exceptions.Any())
        {
            return false;
        }
        if (accountingPeriodRepository.GetByYearAndMonth(request.Year, request.Month) != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountingPeriodRequest.Month)),
                "An Accounting Period already exists for this year and month."));
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountingPeriodRequest.Year)),
                "An Accounting Period already exists for this year and month."));
        }
        exceptions = exceptions.Concat(ValidateExpectedIncomeSources(
            request.ExpectedIncomeSources,
            new AccountingPeriod(request.Year, request.Month)));
        AccountingPeriod? latestAccountingPeriod = accountingPeriodRepository.GetLatestAccountingPeriod();
        if (latestAccountingPeriod != null && latestAccountingPeriod.PeriodStartDate != new DateOnly(request.Year, request.Month, 1).AddMonths(-1))
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountingPeriodRequest.Month)),
                "New Accounting Period must directly follow the most recent existing Accounting Period."));
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAccountingPeriodRequest.Year)),
                "New Accounting Period must directly follow the most recent existing Accounting Period."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Attempts to replace the expected income sources for an open Accounting Period.
    /// </summary>
    public static bool TryReplaceExpectedIncomeSources(
        AccountingPeriod accountingPeriod,
        IReadOnlyCollection<ExpectedIncomeSourceRequest> sources,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];
        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Expected income sources for a closed Accounting Period cannot be changed."));
        }
        exceptions = exceptions.Concat(ValidateExpectedIncomeSources(sources, accountingPeriod));
        if (exceptions.Any())
        {
            return false;
        }
        accountingPeriod.ReplaceExpectedIncomeSources(sources);
        return true;
    }

    /// <summary>
    /// Validates expected income source configuration.
    /// </summary>
    private static IEnumerable<ValidationError> ValidateExpectedIncomeSources(
        IReadOnlyCollection<ExpectedIncomeSourceRequest> sources,
        AccountingPeriod accountingPeriod)
    {
        IEnumerable<ValidationError> errors = [];
        foreach ((int sourceIndex, ExpectedIncomeSourceRequest source) in sources.Index())
        {
            ValidationErrorPath sourcePath = new ValidationErrorPath("").AppendWithIndex(nameof(CreateAccountingPeriodRequest.ExpectedIncomeSources), sourceIndex);
            if (string.IsNullOrWhiteSpace(source.Name))
            {
                errors = errors.Append(new ValidationError(sourcePath.Append(nameof(ExpectedIncomeSourceRequest.Name)), "Expected income source names are required."));
            }
            errors = errors.Concat(IncomeBreakdownValidator.Validate(
                source.Income,
                sourcePath.Append(nameof(ExpectedIncomeSourceRequest.Income))));
            if (source.Income is SimpleIncome)
            {
                errors = errors.Append(new ValidationError(sourcePath.Append(nameof(ExpectedIncomeSourceRequest.Income)), "Expected income must use a payroll breakdown."));
            }
            if (source.Income is ExpectedPayrollPayment expectedPayroll)
            {
                if (expectedPayroll.PayPeriodsPerYear is <= 0 or > 366)
                {
                    errors = errors.Append(new ValidationError(sourcePath.Append(nameof(ExpectedPayrollPayment.PayPeriodsPerYear)), "Expected payroll income must specify between 1 and 366 pay periods per year."));
                }
                if (source.ExpectedDates.Count != 1 || source.ExpectedDates.SingleOrDefault() != expectedPayroll.WithholdingContext.PaymentDate)
                {
                    errors = errors.Append(new ValidationError(
                        sourcePath.Append(nameof(ExpectedIncomeSourceRequest.ExpectedDates)),
                        "An expected payroll projection must identify its single calculated payment date."));
                }
                foreach (DateOnly paymentDate in source.ExpectedDates.DefaultIfEmpty(expectedPayroll.WithholdingContext.PaymentDate))
                {
                    errors = errors.Concat(PayrollWithholdingConfigurationValidator.Validate(
                        expectedPayroll.WithholdingConfiguration,
                        paymentDate,
                        sourcePath.Append(nameof(ExpectedPayrollPayment.WithholdingConfiguration))));
                }
            }
            foreach ((int index, DateOnly date) in source.ExpectedDates.Index())
            {
                if (date.Year != accountingPeriod.Year || date.Month != accountingPeriod.Month)
                {
                    errors = errors.Append(new ValidationError(sourcePath.AppendWithIndex(nameof(ExpectedIncomeSourceRequest.ExpectedDates), index), "Expected income dates must fall within the Accounting Period calendar month."));
                }
            }
            if (source.ExpectedDates.Distinct().Count() != source.ExpectedDates.Count)
            {
                errors = errors.Append(new ValidationError(sourcePath.Append(nameof(ExpectedIncomeSourceRequest.ExpectedDates)), "Expected income dates must be unique within a source."));
            }
        }
        return errors;
    }

    /// <summary>
    /// Validates closing an existing Accounting Period
    /// </summary>
    private bool ValidateClose(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "This Accounting Period is already closed."));
        }
        if (transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id).Any(transaction =>
            transaction.GetAllAffectedAccountIds().Any(accountId => transaction.GetPostedDateForAccount(accountId) == null)))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "There are unposted transactions in this Accounting Period."));
        }
        if (accountingPeriodRepository.GetAllOpenPeriods().Any(openPeriod => openPeriod.PeriodStartDate < accountingPeriod.PeriodStartDate))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "An earlier Accounting Period is still open."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates reopening an existing Accounting Period
    /// </summary>
    private bool ValidateReopen(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "This Accounting Period is already open."));
        }
        AccountingPeriod? nextPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        if (nextPeriod != null && !nextPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "A later Accounting Period is still closed."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates deleting an existing Accounting Period
    /// </summary>
    private bool ValidateDelete(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "This Accounting Period is closed."));
        }
        if (transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id).Count > 0)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "This Accounting Period has transactions."));
        }
        if (accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id) != null)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Deleting this Accounting Period would cause a gap between existing Accounting Periods."));
        }
        if (fundRepository.GetAllFundsAddedInPeriod(accountingPeriod.Id).Any(fund => !fund.IsUnassignedFund))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "This Accounting Period has funds that were added during it."));
        }
        if (accountRepository.GetAllAccountsAddedInPeriod(accountingPeriod.Id).Count > 0)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "This Accounting Period has accounts that were added during it."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Attempts to create the first Accounting Period
    /// </summary>
    private bool TryCreateFirstAccountingPeriod(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (fundRepository.GetUnassignedFund() == null)
        {
            if (!fundService.TryCreateUnassignedFund(accountingPeriod, out Fund? unassignedFund, out IEnumerable<ValidationError> unassignedFundExceptions))
            {
                exceptions = exceptions.Concat(unassignedFundExceptions);
                return false;
            }
            fundRepository.Add(unassignedFund);
        }

        return true;
    }
}