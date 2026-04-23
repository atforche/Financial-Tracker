using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Domain.Funds;

/// <summary>
/// Service for managing Fund Goals
/// </summary>
public class FundGoalService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IFundGoalRepository fundGoalRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Fund Goal in a particular accounting period
    /// </summary>
    public bool TryCreate(CreateFundGoalRequest request, [NotNullWhen(true)] out FundGoal? fundGoal, out IEnumerable<Exception> exceptions)
    {
        fundGoal = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        fundGoal = new FundGoal(
            request.Fund,
            request.AccountingPeriod.Id,
            request.GoalType,
            request.GoalAmount);
        UpdateFundGoalBalances(fundGoal);
        RecalculateFundGoalAmounts(fundGoal);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund Goal
    /// </summary>
    public static bool TryUpdate(
        FundGoal fundGoal,
        FundGoalType goalType,
        decimal goalAmount,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdate(goalType, goalAmount, out IEnumerable<Exception> updateExceptions))
        {
            exceptions = exceptions.Concat(updateExceptions);
            return false;
        }
        fundGoal.GoalType = goalType;
        fundGoal.GoalAmount = goalAmount;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund Goal
    /// </summary>
    public bool TryDelete(FundGoal fundGoal, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];
        fundGoalRepository.Delete(fundGoal);
        return true;
    }

    /// <summary>
    /// Updates the Fund Goals for a newly added Transaction
    /// </summary>
    internal void AddTransaction(Transaction newTransaction)
    {
        foreach (FundId fundId in newTransaction.GetAllAffectedFundIds(null))
        {
            FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(fundId, newTransaction.AccountingPeriodId);
            if (fundGoal == null)
            {
                continue;
            }
            RecalculateFundGoalAmounts(fundGoal);
        }
    }

    /// <summary>
    /// Updates the Fund Goals for an updated Transaction
    /// </summary>
    internal void UpdateTransaction(Transaction transaction)
    {
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null))
        {
            FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(fundId, transaction.AccountingPeriodId);
            if (fundGoal == null)
            {
                continue;
            }
            RecalculateFundGoalAmounts(fundGoal);
        }
    }

    /// <summary>
    /// Updates the Fund Goals for a newly posted Transaction
    /// </summary>
    internal void PostTransaction(Transaction transaction, AccountId accountId)
    {
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(accountId))
        {
            FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(fundId, transaction.AccountingPeriodId);
            if (fundGoal == null)
            {
                continue;
            }
            UpdateFundGoalBalances(fundGoal);
            RecalculateFundGoalAmounts(fundGoal);
        }
    }

    /// <summary>
    /// Updates the Fund Goals for an unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null))
        {
            FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(fundId, transaction.AccountingPeriodId);
            if (fundGoal == null)
            {
                continue;
            }
            UpdateFundGoalBalances(fundGoal);
            RecalculateFundGoalAmounts(fundGoal);
        }
    }

    /// <summary>
    /// Updates the Fund Goals for a deleted Transaction
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null))
        {
            FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(fundId, transaction.AccountingPeriodId);
            if (fundGoal == null)
            {
                continue;
            }
            RecalculateFundGoalAmounts(fundGoal);
        }
    }

    /// <summary>
    /// Validates the provided request to create a fund goal
    /// </summary>
    private bool ValidateCreate(CreateFundGoalRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.Fund.IsSystemFund)
        {
            exceptions = exceptions.Append(new InvalidFundException("The unassigned fund cannot have a fund goal."));
        }
        if (!request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (fundGoalRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod.Id) != null)
        {
            exceptions = exceptions.Append(new InvalidFundException("A fund goal already exists for this fund and accounting period."));
        }
        if (request.GoalAmount <= 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update a fund goal
    /// </summary>
    private static bool ValidateUpdate(FundGoalType goalType, decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(goalType))
        {
            exceptions = exceptions.Append(new InvalidFundGoalTypeException());
        }
        if (goalAmount <= 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Updates the balances for a fund goal
    /// </summary>
    private void UpdateFundGoalBalances(FundGoal fundGoal)
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(fundGoal.AccountingPeriodId);
        while (accountingPeriod != null)
        {
            FundAccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository
                .GetForAccountingPeriod(accountingPeriod.Id)
                .FundBalances.Single(f => f.Fund.Id == fundGoal.Fund.Id);
            fundGoal.OpeningBalance = balanceHistory.GetOpeningFundBalance().PostedBalance;
            fundGoal.ClosingBalance = balanceHistory.GetClosingFundBalance().PostedBalance;
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Recalculates the amounts for a fund goal
    /// </summary>
    private void RecalculateFundGoalAmounts(FundGoal fundGoal)
    {
        decimal amountAssigned = 0;
        decimal pendingAmountAssigned = 0;
        decimal amountSpent = 0;
        decimal pendingAmountSpent = 0;
        foreach (Transaction transaction in transactionRepository.GetAllByAccountingPeriod(fundGoal.AccountingPeriodId)
            .Where(transaction => transaction.GetAllAffectedFundIds(null).Contains(fundGoal.Fund.Id)))
        {
            amountAssigned = CalculateNewAmountAssigned(transaction, fundGoal.Fund, amountAssigned);
            pendingAmountAssigned = CalculateNewPendingAmountAssigned(transaction, fundGoal.Fund, pendingAmountAssigned);
            amountSpent = CalculateNewAmountSpent(transaction, fundGoal.Fund, amountSpent);
            pendingAmountSpent = CalculateNewPendingAmountSpent(transaction, fundGoal.Fund, pendingAmountSpent);
        }
        fundGoal.AmountAssigned = amountAssigned;
        fundGoal.PendingAmountAssigned = pendingAmountAssigned;
        fundGoal.AmountSpent = amountSpent;
        fundGoal.PendingAmountSpent = pendingAmountSpent;
    }

    /// <summary>
    /// Calculates the new amount assigned for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewAmountAssigned(Transaction transaction, Fund fund, decimal existingAmountAssigned)
    {
        if (transaction is IncomeTransaction { CreditPostedDate: { } } incomeTransaction)
        {
            return existingAmountAssigned + incomeTransaction.FundAssignments.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
        }
        return existingAmountAssigned;
    }

    /// <summary>
    /// Calculates the new pending amount assigned for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewPendingAmountAssigned(Transaction transaction, Fund fund, decimal existingPendingAmountAssigned)
    {
        if (transaction is IncomeTransaction { CreditPostedDate: null } incomeTransaction)
        {
            return existingPendingAmountAssigned + incomeTransaction.FundAssignments.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
        }
        return existingPendingAmountAssigned;
    }

    /// <summary>
    /// Calculates the new amount spent for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewAmountSpent(Transaction transaction, Fund fund, decimal existingAmountSpent)
    {
        if (transaction is SpendingTransaction { DebitPostedDate: { } } spendingTransaction)
        {
            return existingAmountSpent + spendingTransaction.FundAssignments.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
        }
        return existingAmountSpent;
    }

    /// <summary>
    /// Calculates the new pending amount spent for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewPendingAmountSpent(Transaction transaction, Fund fund, decimal existingPendingAmountSpent)
    {
        if (transaction is SpendingTransaction { DebitPostedDate: null } spendingTransaction)
        {
            return existingPendingAmountSpent + spendingTransaction.FundAssignments.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
        }
        return existingPendingAmountSpent;
    }
}