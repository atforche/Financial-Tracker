using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Fund Goals
/// </summary>
public class FundGoalService(
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
        FundAccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(request.AccountingPeriod.Id)
            .FundBalances.Single(f => f.Fund.Id == request.Fund.Id);

        decimal amountAssigned = 0;
        decimal pendingAmountAssigned = 0;
        decimal amountSpent = 0;
        decimal pendingAmountSpent = 0;
        foreach (Transaction transaction in transactionRepository.GetAllByAccountingPeriod(request.AccountingPeriod.Id)
            .Where(transaction => transaction.GetAllAffectedFundIds(null).Contains(request.Fund.Id)))
        {
            amountAssigned = CalculateNewAmountAssigned(transaction, request.Fund, amountAssigned);
            pendingAmountAssigned = CalculateNewPendingAmountAssigned(transaction, request.Fund, pendingAmountAssigned);
            amountSpent = CalculateNewAmountSpent(transaction, request.Fund, amountSpent);
            pendingAmountSpent = CalculateNewPendingAmountSpent(transaction, request.Fund, pendingAmountSpent);
        }

        fundGoal = new FundGoal(
            request.Fund,
            request.AccountingPeriod.Id,
            request.GoalType,
            request.GoalAmount,
            balanceHistory.OpeningBalance,
            amountAssigned,
            pendingAmountAssigned,
            amountSpent,
            pendingAmountSpent,
            balanceHistory.ClosingBalance);
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
    /// Calculates the new amount assigned for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewAmountAssigned(Transaction transaction, Fund fund, decimal existingAmountAssigned)
    {
        if (transaction is not IncomeTransaction incomeTransaction || incomeTransaction.PostedDate == null)
        {
            return existingAmountAssigned;
        }
        return existingAmountAssigned + incomeTransaction.FundAmounts.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
    }

    /// <summary>
    /// Calculates the new pending amount assigned for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewPendingAmountAssigned(Transaction transaction, Fund fund, decimal existingPendingAmountAssigned)
    {
        if (transaction is not IncomeTransaction incomeTransaction || incomeTransaction.PostedDate != null)
        {
            return existingPendingAmountAssigned;
        }
        return existingPendingAmountAssigned + incomeTransaction.FundAmounts.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
    }

    /// <summary>
    /// Calculates the new amount spent for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewAmountSpent(Transaction transaction, Fund fund, decimal existingAmountSpent)
    {
        if (transaction is not SpendingTransaction spendingTransaction || spendingTransaction.PostedDate == null)
        {
            return existingAmountSpent;
        }
        return existingAmountSpent + spendingTransaction.FundAmounts.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
    }

    /// <summary>
    /// Calculates the new pending amount spent for a fund goal based on a transaction that affects the fund goal
    /// </summary>
    private static decimal CalculateNewPendingAmountSpent(Transaction transaction, Fund fund, decimal existingPendingAmountSpent)
    {
        if (transaction is not SpendingTransaction spendingTransaction || spendingTransaction.PostedDate != null)
        {
            return existingPendingAmountSpent;
        }
        return existingPendingAmountSpent + spendingTransaction.FundAmounts.Where(fundAmount => fundAmount.FundId == fund.Id).Sum(fundAmount => fundAmount.Amount);
    }
}