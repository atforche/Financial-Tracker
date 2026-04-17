using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Periods
/// </summary>
public class AccountingPeriodService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    IFundGoalRepository fundGoalRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundService fundService,
    FundGoalService fundGoalService)
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
            // This is the first accounting period added to the system, so automatically add the unassigned fund
            if (!fundService.TryCreate(new CreateFundRequest
            {
                Name = "Unassigned",
                Description = "Fund that tracks money that has not been assigned to a specific fund",
                AccountingPeriod = accountingPeriod,
                IsSystemFund = true,
            }, out Fund? unassignedFund, out _, out IEnumerable<Exception> unassignedFundExceptions))
            {
                exceptions = exceptions.Concat(unassignedFundExceptions);
                return false;
            }
            fundRepository.Add(unassignedFund);
        }
        else
        {
            // Automatically carry over all fund goals from the previous accounting period
            foreach (FundGoal fundGoal in fundGoalRepository.GetAllByAccountingPeriod(previousAccountingPeriod.Id))
            {
                var createFundGoalRequest = new CreateFundGoalRequest
                {
                    Fund = fundGoal.Fund,
                    AccountingPeriod = accountingPeriod,
                    GoalType = fundGoal.GoalType,
                    GoalAmount = fundGoal.GoalAmount,
                };
                if (!fundGoalService.TryCreate(createFundGoalRequest, out FundGoal? createdFundGoal, out IEnumerable<Exception> createdFundGoalExceptions))
                {
                    exceptions = exceptions.Concat(createdFundGoalExceptions);
                    return false;
                }
                fundGoalRepository.Add(createdFundGoal);
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
        if (fundRepository.GetAllFundsAddedInPeriod(accountingPeriod.Id).FirstOrDefault(fund => fund.IsSystemFund) is Fund unassignedFund)
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
            // If year or month are invalid, no need to continue validation
            return false;
        }
        // Validate that there are no duplicate accounting periods
        if (accountingPeriodRepository.GetByYearAndMonth(year, month) != null)
        {
            exceptions = exceptions.Append(new InvalidMonthException("An Accounting Period already exists for this year and month."));
            exceptions = exceptions.Append(new InvalidYearException("An Accounting Period already exists for this year and month."));
        }
        // Validate that accounting periods can only be added after existing accounting periods
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
        if (fundRepository.GetAllFundsAddedInPeriod(accountingPeriod.Id).Any(fund => !fund.IsSystemFund))
        {
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period has funds that were added in it."));
        }
        if (accountRepository.GetAllAccountsAddedInPeriod(accountingPeriod.Id).Count > 0)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("This Accounting Period has accounts that were added in it."));
        }
        return !exceptions.Any();
    }
}