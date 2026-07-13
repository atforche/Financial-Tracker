using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Validation;

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
        out IEnumerable<ValidationError> exceptions)
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
            (AccountingPeriodFundBalanceHistory fundBalanceHistory, AccountingPeriodGoalBalanceHistory goalBalanceHistory) =
                GetAccountingPeriodBalanceHistories(assignmentGoal);
            assignmentGoal.EvaluateGoal(fundBalanceHistory, goalBalanceHistory);
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Assignment Goal.
    /// </summary>
    public bool TryUpdate(
        AssignmentGoal assignmentGoal,
        UpdateAssignmentGoalRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateUpdate(assignmentGoal, request, out exceptions))
        {
            return false;
        }

        (AccountingPeriodFundBalanceHistory fundBalanceHistory, AccountingPeriodGoalBalanceHistory goalBalanceHistory) =
            GetAccountingPeriodBalanceHistories(assignmentGoal);
        assignmentGoal.UpdateGoal(request.AssignmentGoalType, request.GoalAmount, fundBalanceHistory, goalBalanceHistory);
        return true;
    }

    /// <summary>
    /// Validates the provided request to create an assignment goal
    /// </summary>
    private bool ValidateCreate(CreateAssignmentGoalRequest request, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (request.Fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAssignmentGoalRequest.Fund)),
                "The unassigned fund cannot have an assignment goal."));
        }
        if (request.AccountingPeriod == null && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAssignmentGoalRequest.AccountingPeriod)),
                "An assignment goal cannot be created without an accounting period if any accounting periods exist."));
        }
        if (request.AccountingPeriod != null && !request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAssignmentGoalRequest.AccountingPeriod)),
                "The provided accounting period is closed."));
        }
        if (assignmentGoalRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod?.Id) != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAssignmentGoalRequest.Fund)),
                "An assignment goal already exists for this fund and accounting period."));
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAssignmentGoalRequest.AccountingPeriod)),
                "An assignment goal already exists for this fund and accounting period."));
        }
        if (request.GoalAmount < 0)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAssignmentGoalRequest.GoalAmount)),
                "Goal amount must be greater than or equal to zero."));
        }
        if (!Enum.IsDefined(request.AssignmentGoalType))
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateAssignmentGoalRequest.AssignmentGoalType)),
                "The provided assignment goal type is invalid."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to update an Assignment Goal.
    /// </summary>
    private bool ValidateUpdate(
        AssignmentGoal assignmentGoal,
        UpdateAssignmentGoalRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(request.AssignmentGoalType))
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateAssignmentGoalRequest.AssignmentGoalType)),
                "The provided assignment goal type is invalid."));
        }
        if (assignmentGoal.AccountingPeriodId == null && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                ValidationErrorPath.Empty,
                "An assignment goal without an accounting period cannot be updated if any accounting periods exist."));
        }
        if (assignmentGoal.AccountingPeriodId != null && !accountingPeriodRepository.GetById(assignmentGoal.AccountingPeriodId)?.IsOpen == true)
        {
            exceptions = exceptions.Append(new ValidationError(
                ValidationErrorPath.Empty,
                "The accounting period for this assignment goal is closed."));
        }
        if (request.GoalAmount < 0)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateAssignmentGoalRequest.GoalAmount)),
                "Goal amount must be greater than or equal to zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Gets the Fund and Goal Balance Histories for a given Assignment Goal
    /// </summary>
    private (AccountingPeriodFundBalanceHistory FundBalanceHistory, AccountingPeriodGoalBalanceHistory GoalBalanceHistory) GetAccountingPeriodBalanceHistories(AssignmentGoal assignmentGoal)
    {
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(
            assignmentGoal.AccountingPeriodId ?? throw new InvalidOperationException("Assignment goal placeholder cannot be evaluated."));
        return (
            balanceHistory.FundBalances.Single(fund => fund.Fund.Id == assignmentGoal.Fund.Id),
            balanceHistory.GoalBalances.Single(goal => goal.Fund.Id == assignmentGoal.Fund.Id));
    }
}