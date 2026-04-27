using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Funds
/// </summary>
public class FundService(
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IFundRepository fundRepository,
    IFundGoalRepository fundGoalRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService)
{
    /// <summary>
    /// Attempts to create a new Fund
    /// </summary>
    public bool TryCreate(
        CreateFundRequest request,
        [NotNullWhen(true)] out Fund? fund,
        out FundGoal? fundGoal,
        out IEnumerable<Exception> exceptions)
    {
        fund = null;
        fundGoal = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        fund = new Fund(request.Name, request.Description, request.AccountingPeriod.Id, request.IsSystemFund);
        accountingPeriodBalanceService.AddFund(fund);
        if (request.GoalType != null && request.GoalAmount != null)
        {
            var createGoalRequest = new CreateFundGoalRequest
            {
                Fund = fund,
                AccountingPeriod = request.AccountingPeriod,
                GoalType = request.GoalType.Value,
                GoalAmount = request.GoalAmount.Value,
            };
            if (!TryCreateGoal(createGoalRequest, out FundGoal? newFundGoal, out IEnumerable<Exception> goalExceptions))
            {
                exceptions = exceptions.Concat(goalExceptions);
                return false;
            }
            fundGoal = newFundGoal;
        }
        return true;
    }

    /// <summary>
    /// Attempts to create a new Fund Goal in a particular accounting period
    /// </summary>
    public bool TryCreateGoal(CreateFundGoalRequest request, [NotNullWhen(true)] out FundGoal? fundGoal, out IEnumerable<Exception> exceptions)
    {
        fundGoal = null;

        if (!ValidateCreateGoal(request, out exceptions))
        {
            return false;
        }
        fundGoal = new FundGoal(
            request.Fund,
            request.AccountingPeriod.Id,
            request.GoalType,
            request.GoalAmount);
        fundGoal.EvaluateGoal(GetAccountingPeriodBalanceHistory(fundGoal));
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund
    /// </summary>
    public bool TryUpdate(Fund fund, string name, string description, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUpdate(fund, name, out exceptions))
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
    public bool TryUpdateGoal(
        FundGoal fundGoal,
        FundGoalType goalType,
        decimal goalAmount,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUpdateGoal(goalType, goalAmount, out IEnumerable<Exception> updateExceptions))
        {
            exceptions = exceptions.Concat(updateExceptions);
            return false;
        }
        fundGoal.UpdateGoal(goalType, goalAmount, GetAccountingPeriodBalanceHistory(fundGoal));
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund
    /// </summary>
    public bool TryDelete(Fund fund, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(fund, out exceptions))
        {
            return false;
        }
        accountingPeriodBalanceService.DeleteFund(fund);
        fundRepository.Delete(fund);
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund Goal
    /// </summary>
    public bool TryDeleteGoal(FundGoal fundGoal, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];
        fundGoalRepository.Delete(fundGoal);
        return true;
    }

    /// <summary>
    /// Validates the name for a Fund
    /// </summary>
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
    /// Validates the provided request to create a fund
    /// </summary>
    private bool ValidateCreate(CreateFundRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(request.Name, null, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (request.IsSystemFund && fundRepository.GetSystemFund() != null)
        {
            exceptions = exceptions.Append(new InvalidFundException("An unassigned fund already exists."));
        }
        if (request.GoalAmount == null && request.GoalType != null)
        {
            exceptions = exceptions.Append(new InvalidFundGoalTypeException("A goal type cannot be provided without a goal amount."));
        }
        if (request.GoalAmount != null && request.GoalType == null)
        {
            exceptions = exceptions.Append(new InvalidFundGoalTypeException("A goal type must be provided when creating an initial fund goal."));
        }
        if (!request.AccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to create a fund goal
    /// </summary>
    private bool ValidateCreateGoal(CreateFundGoalRequest request, out IEnumerable<Exception> exceptions)
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
    /// Validates the provided information to update a fund
    /// </summary>
    private bool ValidateUpdate(Fund fund, string name, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(name, fund, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (fund.IsSystemFund)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("The unassigned fund cannot be updated."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update a fund goal
    /// </summary>
    private static bool ValidateUpdateGoal(FundGoalType goalType, decimal goalAmount, out IEnumerable<Exception> exceptions)
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
    /// Validates whether a fund can be deleted
    /// </summary>
    private bool ValidateDelete(Fund fund, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (fund.IsSystemFund)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("The unassigned fund cannot be deleted."));
        }
        if (transactionRepository.DoAnyTransactionsExistForFund(fund.Id))
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Cannot delete a Fund that has Transactions."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Gets the Accounting Period Balance History for a given Fund Goal
    /// </summary>
    private AccountingPeriodFundBalanceHistory GetAccountingPeriodBalanceHistory(FundGoal fundGoal) =>
        accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(fundGoal.AccountingPeriodId)
                .FundBalances.Single(fund => fund.Id == fundGoal.Fund.Id);
}