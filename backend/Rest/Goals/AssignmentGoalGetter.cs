using Data.Goals;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;
using Models;
using Models.Goals;
using Rest.AccountingPeriods;
using Rest.Funds;

namespace Rest.Goals;

/// <summary>
/// Class that handles retrieving Assignment Goals based on specified criteria
/// </summary>
public class AssignmentGoalGetter(
    AssignmentGoalRepository goalRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    FundConverter fundConverter,
    GoalConverter goalConverter,
    IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Gets the Assignment Goals that match the specified criteria
    /// </summary>
    public bool TryGet(
        AssignmentGoalQueryParameterModel request,
        out CollectionModel<AssignmentGoalModel> results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        List<AccountingPeriodId> accountingPeriodIds = [];
        foreach (Guid accountingPeriodId in request.AccountingPeriodIds ?? [])
        {
            if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
            {
                AddError(errors, nameof(request.AccountingPeriodIds), $"Accounting Period with ID {accountingPeriodId} was not found.");
            }
            else
            {
                accountingPeriodIds.Add(accountingPeriod.Id);
            }
        }

        List<FundId> fundIds = [];
        foreach (Guid fundId in request.FundIds ?? [])
        {
            if (!fundConverter.TryToDomain(fundId, out Fund? fund))
            {
                AddError(errors, nameof(request.FundIds), $"Fund with ID {fundId} was not found.");
            }
            else
            {
                fundIds.Add(fund.Id);
            }
        }

        IEnumerable<AssignmentGoal> goals = accountingPeriodIds.Count == 0
            ? goalRepository.GetAll()
            : accountingPeriodIds.SelectMany(goalRepository.GetAllByAccountingPeriod);
        if (accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            goals = goals.Where(goal => goal.AccountingPeriodId != null);
        }

        if (fundIds.Count > 0)
        {
            goals = goals.Where(goal => fundIds.Contains(goal.Fund.Id));
        }

        var resultsList = goals.Select(goalConverter.ToModel).ToList();
        if (request.Sort is null or AssignmentGoalSortOrderModel.Fund)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.FundDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.AccountingPeriod)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.AccountingPeriodName)
                .ThenBy(goal => goal.FundName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.AccountingPeriodDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.AccountingPeriodName)
                .ThenByDescending(goal => goal.FundName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.Type)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.Type)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.TypeDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.Type)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.GoalAmount)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.GoalAmount)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.GoalAmountDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.GoalAmount)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.TotalAmountToAssign)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.TotalAmountToAssign)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.TotalAmountToAssignDescending)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.TotalAmountToAssign)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.TotalAmountAssigned)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.TotalAmountAssigned)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.TotalAmountAssignedDescending)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.TotalAmountAssigned)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.IsMet)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.IsGoalMet)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == AssignmentGoalSortOrderModel.IsMetDescending)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.IsGoalMet)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        results = new CollectionModel<AssignmentGoalModel>
        {
            Items = resultsList.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = resultsList.Count,
        };
        return errors.Count == 0;
    }

    private static void AddError(Dictionary<string, string[]> errors, string key, string message)
    {
        if (errors.TryGetValue(key, out string[]? existing))
        {
            errors[key] = existing.Concat([message]).ToArray();
        }
        else
        {
            errors.Add(key, [message]);
        }
    }
}