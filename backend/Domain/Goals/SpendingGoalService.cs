using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;

namespace Domain.Goals;

/// <summary>
/// Service for managing Spending Goals
/// </summary>
public class SpendingGoalService(
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ISpendingGoalRepository spendingGoalRepository)
{
    /// <summary>
    /// Attempts to create a new Spending Goal in a particular accounting period
    /// </summary>
    public bool TryCreate(
        CreateSpendingGoalRequest request,
        [NotNullWhen(true)] out SpendingGoal? spendingGoal,
        out IEnumerable<Exception> exceptions)
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
    /// Attempts to update an existing Spending Goal
    /// </summary>
    public bool TryUpdate(
        SpendingGoal spendingGoal,
        SpendingGoalType spendingGoalType,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdate(spendingGoalType, out IEnumerable<Exception> updateExceptions))
        {
            exceptions = exceptions.Concat(updateExceptions);
            return false;
        }

        spendingGoal.UpdateGoal(spendingGoalType, GetAccountingPeriodBalanceHistory(spendingGoal));
        return true;
    }

    /// <summary>
    /// Validates the provided request to create a spending goal
    /// </summary>
    private bool ValidateCreate(CreateSpendingGoalRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.Fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new InvalidFundException("The unassigned fund cannot have a spending goal."));
        }
        if (request.AccountingPeriod != null && !request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (spendingGoalRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod?.Id) != null)
        {
            exceptions = exceptions.Append(new InvalidFundException("A spending goal already exists for this fund and accounting period."));
        }
        if (!Enum.IsDefined(request.SpendingGoalType))
        {
            exceptions = exceptions.Append(new InvalidGoalTypeException("The provided spending goal type is invalid."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update a spending goal
    /// </summary>
    private static bool ValidateUpdate(SpendingGoalType spendingGoalType, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(spendingGoalType))
        {
            exceptions = exceptions.Append(new InvalidGoalTypeException("The provided spending goal type is invalid."));
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