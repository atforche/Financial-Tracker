using System.Diagnostics.CodeAnalysis;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Budgets;

/// <summary>
/// Service for managing Budgets and Budget Goals
/// </summary>
public class BudgetService(
    IBudgetRepository budgetRepository,
    IBudgetGoalRepository budgetGoalRepository,
    IFundRepository fundRepository)
{
    /// <summary>
    /// Attempts to create a new Budget
    /// </summary>
    public bool TryCreate(CreateBudgetRequest request, [NotNullWhen(true)] out Budget? budget, out IEnumerable<Exception> exceptions)
    {
        budget = null;
        exceptions = [];

        if (!ValidateCreate(request, out IEnumerable<Exception> createExceptions))
        {
            exceptions = exceptions.Concat(createExceptions);
            return false;
        }
        budget = new Budget(request.Name, request.Description, request.Type, request.FundId);
        budgetRepository.Add(budget);
        return true;
    }

    /// <summary>
    /// Attempts to create a new Budget Goal in a particular accounting period
    /// </summary>
    public bool TryCreateGoal(CreateBudgetGoalRequest request, [NotNullWhen(true)] out BudgetGoal? budgetGoal, out IEnumerable<Exception> exceptions)
    {
        budgetGoal = null;
        exceptions = [];

        if (!ValidateCreateGoal(request, out IEnumerable<Exception> createGoalExceptions))
        {
            exceptions = exceptions.Concat(createGoalExceptions);
            return false;
        }
        budgetGoal = new BudgetGoal(request.Budget, request.AccountingPeriod.Id, request.GoalAmount);
        budgetGoalRepository.Add(budgetGoal);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Budget
    /// </summary>
    public bool TryUpdate(Budget budget, string name, string description, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdate(budget, name, out IEnumerable<Exception> updateExceptions))
        {
            exceptions = exceptions.Concat(updateExceptions);
            return false;
        }
        budget.Name = name;
        budget.Description = description;
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Budget Goal
    /// </summary>
    public static bool TryUpdateGoal(BudgetGoal budgetGoal, decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdateGoal(goalAmount, out IEnumerable<Exception> updateGoalExceptions))
        {
            exceptions = exceptions.Concat(updateGoalExceptions);
            return false;
        }
        budgetGoal.GoalAmount = goalAmount;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Budget
    /// </summary>
    public bool TryDelete(Budget budget, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateDelete(budget, out IEnumerable<Exception> deleteExceptions))
        {
            exceptions = exceptions.Concat(deleteExceptions);
            return false;
        }
        budgetRepository.Delete(budget);
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Budget Goal
    /// </summary>
    public bool TryDeleteGoal(BudgetGoal budgetGoal, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateDeleteGoal(budgetGoal, out IEnumerable<Exception> deleteGoalExceptions))
        {
            exceptions = exceptions.Concat(deleteGoalExceptions);
            return false;
        }
        budgetGoalRepository.Delete(budgetGoal);
        return true;
    }

    /// <summary>
    /// Validates the provided request to create a budget
    /// </summary>
    private bool ValidateCreate(CreateBudgetRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(request.Name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Budget name cannot be empty."));
        }
        if (budgetRepository.TryGetByName(request.Name, out _) && !string.IsNullOrEmpty(request.Name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Budget name must be unique."));
        }
        if (!fundRepository.GetAll().Any(f => f.Id == request.FundId))
        {
            exceptions = exceptions.Append(new InvalidBudgetException("The linked fund does not exist."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to create a budget goal
    /// </summary>
    private bool ValidateCreateGoal(CreateBudgetGoalRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (budgetGoalRepository.GetByBudgetAndAccountingPeriod(request.Budget.Id, request.AccountingPeriod.Id) != null)
        {
            exceptions = exceptions.Append(new InvalidBudgetException("A budget goal already exists for this budget and accounting period."));
        }
        if (request.GoalAmount <= 0)
        {
            exceptions = exceptions.Append(new InvalidBudgetException("Goal amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update a budget
    /// </summary>
    private bool ValidateUpdate(Budget budget, string name, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Budget name cannot be empty."));
        }
        if (budgetRepository.TryGetByName(name, out Budget? existingBudgetWithName) && existingBudgetWithName != budget)
        {
            exceptions = exceptions.Append(new InvalidNameException("Budget name must be unique."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update a budget goal
    /// </summary>
    private static bool ValidateUpdateGoal(decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (goalAmount <= 0)
        {
            exceptions = exceptions.Append(new InvalidBudgetException("Goal amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to delete a budget
    /// </summary>
    private bool ValidateDelete(Budget budget, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (budgetGoalRepository.GetAllByBudget(budget.Id).Any(g => g.BudgetBalanceHistories.Count > 0))
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Cannot delete a Budget that has transactions linked to it."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to delete a budget goal
    /// </summary>
    private static bool ValidateDeleteGoal(BudgetGoal budgetGoal, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (budgetGoal.BudgetBalanceHistories.Count > 0)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Cannot delete a Budget Goal that has transactions linked to it."));
        }
        return !exceptions.Any();
    }
}
