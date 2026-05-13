using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;

namespace Domain.Goals;

/// <summary>
/// Service for managing Goals
/// </summary>
public class GoalService(
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IGoalRepository goalRepository)
{
    /// <summary>
    /// Attempts to create a new Goal in a particular accounting period
    /// </summary>
    public bool TryCreate(CreateGoalRequest request, [NotNullWhen(true)] out Goal? goal, out IEnumerable<Exception> exceptions)
    {
        goal = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }

        goal = new Goal(
            request.Fund,
            request.AccountingPeriod.Id,
            request.GoalType,
            request.GoalAmount);
        goal.EvaluateGoal(GetAccountingPeriodBalanceHistory(goal));
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Goal
    /// </summary>
    public bool TryUpdate(Goal goal, GoalType goalType, decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdate(goalType, goalAmount, out IEnumerable<Exception> updateExceptions))
        {
            exceptions = exceptions.Concat(updateExceptions);
            return false;
        }

        goal.UpdateGoal(goalType, goalAmount, GetAccountingPeriodBalanceHistory(goal));
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Goal
    /// </summary>
    public bool TryDelete(Goal goal, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];
        goalRepository.Delete(goal);
        return true;
    }

    /// <summary>
    /// Validates the provided request to create a goal
    /// </summary>
    private bool ValidateCreate(CreateGoalRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.Fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new InvalidFundException("The unassigned fund cannot have a goal."));
        }
        if (!request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (goalRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod.Id) != null)
        {
            exceptions = exceptions.Append(new InvalidFundException("A goal already exists for this fund and accounting period."));
        }
        if (request.GoalAmount <= 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update a goal
    /// </summary>
    private static bool ValidateUpdate(GoalType goalType, decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(goalType))
        {
            exceptions = exceptions.Append(new InvalidGoalTypeException());
        }
        if (goalAmount <= 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Gets the Accounting Period Balance History for a given Goal
    /// </summary>
    private AccountingPeriodFundBalanceHistory GetAccountingPeriodBalanceHistory(Goal goal) =>
        accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(goal.AccountingPeriodId)
            .FundBalances.Single(fund => fund.Fund.Id == goal.Fund.Id);
}