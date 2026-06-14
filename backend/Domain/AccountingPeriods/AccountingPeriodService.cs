using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Goals;
using Domain.Transactions;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Periods
/// </summary>
public class AccountingPeriodService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    IAssignmentGoalRepository assignmentGoalRepository,
    ISpendingGoalRepository spendingGoalRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundService fundService,
    AssignmentGoalService assignmentGoalService,
    SpendingGoalService spendingGoalService)
{
    /// <summary>
    /// Attempts to create a new Accounting Period
    /// </summary>
    public bool TryCreate(
        int year,
        int month,
        [NotNullWhen(true)] out AccountingPeriod? accountingPeriod,
        out IEnumerable<Exception> exceptions)
    {
        accountingPeriod = null;

        if (!ValidateCreate(year, month, out exceptions))
        {
            return false;
        }
        accountingPeriod = new AccountingPeriod(year, month);
        accountingPeriodBalanceService.AddAccountingPeriod(accountingPeriod);

        AccountingPeriod? previousAccountingPeriod = accountingPeriodRepository.GetPreviousAccountingPeriod(accountingPeriod.Id);
        if (previousAccountingPeriod == null)
        {
            if (fundRepository.GetUnassignedFund() == null)
            {
                if (!fundService.TryCreateUnassignedFund(accountingPeriod, out Fund? unassignedFund, out IEnumerable<Exception> unassignedFundExceptions))
                {
                    exceptions = exceptions.Concat(unassignedFundExceptions);
                    return false;
                }
                fundRepository.Add(unassignedFund);
            }

            foreach (Fund fund in fundRepository.GetAll().Where(fund => !fund.IsUnassignedFund && fund.IsOnboarded))
            {
                AssignmentGoal? assignmentGoalPlaceholder = assignmentGoalRepository.GetByFundAndAccountingPeriod(fund.Id, null);
                SpendingGoal? spendingGoalPlaceholder = spendingGoalRepository.GetByFundAndAccountingPeriod(fund.Id, null);
                if (assignmentGoalPlaceholder == null || spendingGoalPlaceholder == null)
                {
                    exceptions = exceptions.Append(new InvalidFundException($"The onboarded fund '{fund.Name}' is missing placeholder goals."));
                    return false;
                }

                var createAssignmentGoalRequest = new CreateAssignmentGoalRequest
                {
                    Fund = fund,
                    AccountingPeriod = accountingPeriod,
                    AssignmentGoalType = assignmentGoalPlaceholder.AssignmentGoalType,
                    GoalAmount = assignmentGoalPlaceholder.GoalAmount,
                };
                if (!assignmentGoalService.TryCreate(
                    createAssignmentGoalRequest,
                    out AssignmentGoal? createdAssignmentGoal,
                    out IEnumerable<Exception> createdAssignmentGoalExceptions))
                {
                    exceptions = exceptions.Concat(createdAssignmentGoalExceptions);
                    return false;
                }
                assignmentGoalRepository.Add(createdAssignmentGoal);

                var createSpendingGoalRequest = new CreateSpendingGoalRequest
                {
                    Fund = fund,
                    AccountingPeriod = accountingPeriod,
                    SpendingGoalType = spendingGoalPlaceholder.SpendingGoalType,
                };
                if (!spendingGoalService.TryCreate(
                    createSpendingGoalRequest,
                    out SpendingGoal? createdSpendingGoal,
                    out IEnumerable<Exception> createdSpendingGoalExceptions))
                {
                    exceptions = exceptions.Concat(createdSpendingGoalExceptions);
                    return false;
                }
                spendingGoalRepository.Add(createdSpendingGoal);
            }
        }
        else
        {
            // Automatically carry over all fund assignment goals from the previous accounting period
            foreach (AssignmentGoal assignmentGoal in assignmentGoalRepository.GetAllByAccountingPeriod(previousAccountingPeriod.Id))
            {
                var createAssignmentGoalRequest = new CreateAssignmentGoalRequest
                {
                    Fund = assignmentGoal.Fund,
                    AccountingPeriod = accountingPeriod,
                    AssignmentGoalType = assignmentGoal.AssignmentGoalType,
                    GoalAmount = assignmentGoal.TotalAmountToAssign,
                };
                if (!assignmentGoalService.TryCreate(
                    createAssignmentGoalRequest,
                    out AssignmentGoal? createdAssignmentGoal,
                    out IEnumerable<Exception> createdAssignmentGoalExceptions))
                {
                    exceptions = exceptions.Concat(createdAssignmentGoalExceptions);
                    return false;
                }
                assignmentGoalRepository.Add(createdAssignmentGoal);
            }

            // Automatically carry over all fund spending goals from the previous accounting period
            foreach (SpendingGoal spendingGoal in spendingGoalRepository.GetAllByAccountingPeriod(previousAccountingPeriod.Id))
            {
                var createSpendingGoalRequest = new CreateSpendingGoalRequest
                {
                    Fund = spendingGoal.Fund,
                    AccountingPeriod = accountingPeriod,
                    SpendingGoalType = spendingGoal.SpendingGoalType,
                };
                if (!spendingGoalService.TryCreate(
                    createSpendingGoalRequest,
                    out SpendingGoal? createdSpendingGoal,
                    out IEnumerable<Exception> createdSpendingGoalExceptions))
                {
                    exceptions = exceptions.Concat(createdSpendingGoalExceptions);
                    return false;
                }
                spendingGoalRepository.Add(createdSpendingGoal);
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to close an existing Accounting Period
    /// </summary>
    public bool TryClose(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
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
    public bool TryReopen(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
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
    public bool TryDelete(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(accountingPeriod, out exceptions))
        {
            return false;
        }
        accountingPeriodBalanceService.DeleteAccountingPeriod(accountingPeriod);
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
    private bool ValidateCreate(int year, int month, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (year is < 2000 or > 2100)
        {
            exceptions = exceptions.Append(new InvalidYearException("The provided year must be between 2000 and 2100."));
        }
        if (month is <= 0 or > 12)
        {
            exceptions = exceptions.Append(new InvalidMonthException("The provided month must be between 1 and 12."));
        }
        if (exceptions.Any())
        {
            return false;
        }
        if (accountingPeriodRepository.GetByYearAndMonth(year, month) != null)
        {
            exceptions = exceptions.Append(new InvalidMonthException("An Accounting Period already exists for this year and month."));
            exceptions = exceptions.Append(new InvalidYearException("An Accounting Period already exists for this year and month."));
        }
        AccountingPeriod? latestAccountingPeriod = accountingPeriodRepository.GetLatestAccountingPeriod();
        if (latestAccountingPeriod != null && latestAccountingPeriod.PeriodStartDate != new DateOnly(year, month, 1).AddMonths(-1))
        {
            exceptions = exceptions.Append(new InvalidMonthException("New Accounting Period must directly follow the most recent existing Accounting Period."));
            exceptions = exceptions.Append(new InvalidYearException("New Accounting Period must directly follow the most recent existing Accounting Period."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates closing an existing Accounting Period
    /// </summary>
    private bool ValidateClose(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new UnableToCloseException("This Accounting Period is already closed."));
        }
        if (transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id).Any(transaction =>
            transaction.GetAllAffectedAccountIds().Any(accountId => transaction.GetPostedDateForAccount(accountId) == null)))
        {
            exceptions = exceptions.Append(new UnableToCloseException("There are unposted transactions in this Accounting Period."));
        }
        if (accountingPeriodRepository.GetAllOpenPeriods().Any(openPeriod => openPeriod.PeriodStartDate < accountingPeriod.PeriodStartDate))
        {
            exceptions = exceptions.Append(new UnableToCloseException("An earlier Accounting Period is still open."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates reopening an existing Accounting Period
    /// </summary>
    private bool ValidateReopen(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new UnableToReopenException("This Accounting Period is already open."));
        }
        AccountingPeriod? nextPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        if (nextPeriod != null && !nextPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new UnableToReopenException("A later Accounting Period is still closed."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates deleting an existing Accounting Period
    /// </summary>
    private bool ValidateDelete(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period is closed."));
        }
        if (transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id).Count > 0)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period has transactions."));
        }
        if (accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id) != null)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Deleting this Accounting Period would cause a gap between existing Accounting Periods."));
        }
        if (fundRepository.GetAllFundsAddedInPeriod(accountingPeriod.Id).Any(fund => !fund.IsUnassignedFund))
        {
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period has funds that were added during it."));
        }
        if (accountRepository.GetAllAccountsAddedInPeriod(accountingPeriod.Id).Count > 0)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period has accounts that were added during it."));
        }
        return !exceptions.Any();
    }
}