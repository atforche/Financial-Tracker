using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Funds
/// </summary>
public class FundService(
    IFundRepository fundRepository,
    IFundGoalRepository fundGoalRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService)
{
    /// <summary>
    /// Attempts to create a new Fund
    /// </summary>
    /// <param name="request">Request to create a Fund</param>
    /// <param name="fund">The created Fund, or null if creation failed</param>
    /// <param name="exceptions">List of exceptions encountered during creation</param>
    /// <returns>True if the Fund was created successfully, false otherwise</returns>
    public bool TryCreate(CreateFundRequest request, [NotNullWhen(true)] out Fund? fund, out IEnumerable<Exception> exceptions)
    {
        fund = null;
        exceptions = [];

        if (!ValidateName(request.Name, null, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (!request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (!request.AccountingPeriod.IsDateInPeriod(request.AddDate))
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided add date is not within the provided accounting period."));
        }
        if (exceptions.Any())
        {
            return false;
        }
        fund = new Fund(request.Name, request.Type, request.Description, request.AccountingPeriod.Id, request.AddDate);
        accountingPeriodBalanceService.AddFund(fund);
        return true;
    }

    /// <summary>
    /// Attempts to create a new Fund Goal in a particular accounting period
    /// </summary>
    public bool TryCreateGoal(CreateFundGoalRequest request, [NotNullWhen(true)] out FundGoal? fundGoal, out IEnumerable<Exception> exceptions)
    {
        fundGoal = null;
        exceptions = [];

        if (!ValidateCreateGoal(request, out IEnumerable<Exception> createGoalExceptions))
        {
            exceptions = exceptions.Concat(createGoalExceptions);
            return false;
        }
        fundGoal = new FundGoal(request.Fund, request.AccountingPeriod.Id, request.GoalAmount);
        fundGoalRepository.Add(fundGoal);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund
    /// </summary>
    /// <param name="fund">Fund to be updated</param>
    /// <param name="name">New name for the Fund</param>
    /// <param name="description">New description for the Fund</param>
    /// <param name="exceptions">List of exceptions encountered during update</param>
    /// <returns>True if the Fund was updated successfully, false otherwise</returns>
    public bool TryUpdate(Fund fund, string name, string description, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(name, fund, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        fund.Name = name;
        fund.Description = description;
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund Goal
    /// </summary>
    public static bool TryUpdateGoal(FundGoal fundGoal, decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdateGoal(goalAmount, out IEnumerable<Exception> updateGoalExceptions))
        {
            exceptions = exceptions.Concat(updateGoalExceptions);
            return false;
        }
        fundGoal.GoalAmount = goalAmount;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund
    /// </summary>
    /// <param name="fund">Fund to be deleted</param>
    /// <param name="exceptions">List of exceptions encountered during deletion</param>
    /// <returns>True if the Fund was deleted successfully, false otherwise</returns>
    public bool TryDelete(Fund fund, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (transactionRepository.DoAnyTransactionsExistForFund(fund.Id))
        {
            exceptions = [new UnableToDeleteException("Cannot delete a Fund that has Transactions.")];
            return false;
        }
        accountingPeriodBalanceService.DeleteFund(fund);
        fundRepository.Delete(fund);
        return true;
    }

    /// <summary>
    /// Validates the name for a Fund
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <param name="existingFund">The existing Fund being updated, if any</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if this name is valid for a Fund, false otherwise</returns>
    private bool ValidateName(string name, Fund? existingFund, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Fund name cannot be empty"));
        }
        if (fundRepository.TryGetByName(name, out Fund? existingFundWithName) && existingFundWithName != existingFund)
        {
            exceptions = exceptions.Append(new InvalidNameException("Fund name must be unique"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to create a fund goal
    /// </summary>
    private bool ValidateCreateGoal(CreateFundGoalRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

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
    private static bool ValidateUpdateGoal(decimal goalAmount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (goalAmount <= 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than zero."));
        }
        return !exceptions.Any();
    }
}