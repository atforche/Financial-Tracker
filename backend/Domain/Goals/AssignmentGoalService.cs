using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;

namespace Domain.Goals;

/// <summary>
/// Service for managing Assignment Goals
/// </summary>
public class AssignmentGoalService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IAssignmentGoalRepository assignmentGoalRepository)
{
    /// <summary>
    /// Attempts to create a new Assignment Goal in a particular accounting period
    /// </summary>
    public bool TryCreate(
        CreateAssignmentGoalRequest request,
        [NotNullWhen(true)] out AssignmentGoal? assignmentGoal,
        out IEnumerable<Exception> exceptions)
    {
        assignmentGoal = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        assignmentGoal = new AssignmentGoal(
            request.Fund,
            request.AccountingPeriod?.Id,
            request.AssignmentGoalType,
            request.GoalAmount);
        if (request.AccountingPeriod != null)
        {
            assignmentGoal.EvaluateGoal(GetAccountingPeriodBalanceHistory(assignmentGoal));
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Assignment Goal
    /// </summary>
    public bool TryUpdate(
        AssignmentGoal assignmentGoal,
        AssignmentGoalType assignmentGoalType,
        decimal goalAmount,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdate(assignmentGoal, assignmentGoalType, goalAmount, out IEnumerable<Exception> updateExceptions))
        {
            exceptions = exceptions.Concat(updateExceptions);
            return false;
        }
        assignmentGoal.UpdateGoal(assignmentGoalType, goalAmount, GetAccountingPeriodBalanceHistory(assignmentGoal));
        return true;
    }

    /// <summary>
    /// Validates the provided request to create an assignment goal
    /// </summary>
    private bool ValidateCreate(CreateAssignmentGoalRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.Fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new InvalidFundException("The unassigned fund cannot have an assignment goal."));
        }
        if (request.AccountingPeriod == null && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("An assignment goal cannot be created without an accounting period if any accounting periods exist."));
        }
        if (request.AccountingPeriod != null && !request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (assignmentGoalRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod?.Id) != null)
        {
            exceptions = exceptions.Append(new InvalidFundException("An assignment goal already exists for this fund and accounting period."));
        }
        if (request.GoalAmount < 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than or equal to zero."));
        }
        if (!Enum.IsDefined(request.AssignmentGoalType))
        {
            exceptions = exceptions.Append(new InvalidGoalTypeException("The provided assignment goal type is invalid."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update an assignment goal
    /// </summary>
    private bool ValidateUpdate(AssignmentGoal assignmentGoal, AssignmentGoalType assignmentGoalType, decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(assignmentGoalType))
        {
            exceptions = exceptions.Append(new InvalidGoalTypeException("The provided assignment goal type is invalid."));
        }
        if (assignmentGoal.AccountingPeriodId == null && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("An assignment goal without an accounting period cannot be updated if any accounting periods exist."));
        }
        if (assignmentGoal.AccountingPeriodId != null && !accountingPeriodRepository.GetById(assignmentGoal.AccountingPeriodId)?.IsOpen == true)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The accounting period for this assignment goal is closed."));
        }
        if (goalAmount < 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than or equal to zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Gets the Accounting Period Balance History for a given Assignment Goal
    /// </summary>
    private AccountingPeriodFundBalanceHistory GetAccountingPeriodBalanceHistory(AssignmentGoal assignmentGoal) =>
        accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(assignmentGoal.AccountingPeriodId ?? throw new InvalidOperationException("Assignment goal placeholder cannot be evaluated."))
            .FundBalances.Single(fund => fund.Fund.Id == assignmentGoal.Fund.Id);
}