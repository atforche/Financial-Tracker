using System.Diagnostics.CodeAnalysis;
using Domain.Exceptions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Fund Goals
/// </summary>
public class FundGoalService(IFundGoalRepository fundGoalRepository)
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
        fundGoal = new FundGoal(request.Fund, request.AccountingPeriod.Id, request.GoalType, request.GoalAmount);
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
}