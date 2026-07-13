using System.Diagnostics.CodeAnalysis;
using Domain.Validation;
using Domain.AccountingPeriods;

namespace Domain.Goals;

/// <summary>
/// Service for managing Spending Goals
/// </summary>
public class SpendingGoalService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ISpendingGoalRepository spendingGoalRepository)
{
    /// <summary>
    /// Attempts to create a new Spending Goal in a particular accounting period
    /// </summary>
    public bool TryCreate(
        CreateSpendingGoalRequest request,
        [NotNullWhen(true)] out SpendingGoal? spendingGoal,
        out IEnumerable<ValidationError> exceptions)
    {
        spendingGoal = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        spendingGoal = new SpendingGoal(
            request.Fund,
            request.AccountingPeriod?.Id,
            request.SpendingGoalType);
        if (request.AccountingPeriod != null)
        {
            spendingGoal.EvaluateGoal(GetAccountingPeriodBalanceHistory(spendingGoal));
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Spending Goal.
    /// </summary>
    public bool TryUpdate(
        SpendingGoal spendingGoal,
        UpdateSpendingGoalRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateUpdate(spendingGoal, request, out exceptions))
        {
            return false;
        }

        spendingGoal.UpdateGoal(request.SpendingGoalType, GetAccountingPeriodBalanceHistory(spendingGoal));
        return true;
    }

    /// <summary>
    /// Validates the provided request to create a spending goal
    /// </summary>
    private bool ValidateCreate(CreateSpendingGoalRequest request, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (request.Fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateSpendingGoalRequest.Fund)),
                "The unassigned fund cannot have a spending goal."));
        }
        if (request.AccountingPeriod == null && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateSpendingGoalRequest.AccountingPeriod)),
                "A spending goal without an accounting period cannot be created if any accounting periods exist."));
        }
        if (request.AccountingPeriod != null && !request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateSpendingGoalRequest.AccountingPeriod)),
                "The provided accounting period is closed."));
        }
        if (spendingGoalRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod?.Id) != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateSpendingGoalRequest.Fund)),
                "A spending goal already exists for this fund and accounting period."));
        }
        if (!Enum.IsDefined(request.SpendingGoalType))
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateSpendingGoalRequest.SpendingGoalType)),
                "The provided spending goal type is invalid."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to update a Spending Goal.
    /// </summary>
    private bool ValidateUpdate(
        SpendingGoal spendingGoal,
        UpdateSpendingGoalRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(request.SpendingGoalType))
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateSpendingGoalRequest.SpendingGoalType)),
                "The provided spending goal type is invalid."));
        }
        if (spendingGoal.AccountingPeriodId == null && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                ValidationErrorPath.Empty,
                "A spending goal without an accounting period cannot be updated if any accounting periods exist."));
        }
        if (spendingGoal.AccountingPeriodId != null && !accountingPeriodRepository.GetById(spendingGoal.AccountingPeriodId)?.IsOpen == true)
        {
            exceptions = exceptions.Append(new ValidationError(
                ValidationErrorPath.Empty,
                "The accounting period for this spending goal is closed."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Gets the Accounting Period Balance History for a given Spending Goal
    /// </summary>
    private AccountingPeriodFundBalanceHistory GetAccountingPeriodBalanceHistory(SpendingGoal spendingGoal) =>
        accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(spendingGoal.AccountingPeriodId ?? throw new InvalidOperationException("Spending goal placeholder cannot be evaluated."))
            .FundBalances.Single(fund => fund.Fund.Id == spendingGoal.Fund.Id);
}