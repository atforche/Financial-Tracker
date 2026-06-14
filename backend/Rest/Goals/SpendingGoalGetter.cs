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
/// Class that handles retrieving Spending Goals based on specified criteria
/// </summary>
public class SpendingGoalGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    SpendingGoalRepository goalRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    FundConverter fundConverter,
    GoalConverter goalConverter)
{
    /// <summary>
    /// Gets the Spending Goals that match the specified criteria
    /// </summary>
    public bool TryGet(
        SpendingGoalQueryParameterModel request,
        out CollectionModel<SpendingGoalModel> results,
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

        IEnumerable<SpendingGoal> goals = accountingPeriodIds.Count == 0
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
        if (request.Sort is null or SpendingGoalSortOrderModel.Fund)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.FundDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.AccountingPeriod)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.AccountingPeriodName)
                .ThenBy(goal => goal.FundName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.AccountingPeriodDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.AccountingPeriodName)
                .ThenByDescending(goal => goal.FundName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.Type)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.Type)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.TypeDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.Type)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.TotalAmountToSpend)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.TotalAmountToSpend)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.TotalAmountToSpendDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.TotalAmountToSpend)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.TotalAmountSpent)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.TotalAmountSpent)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.TotalAmountSpentDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.TotalAmountSpent)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.IsMet)
        {
            resultsList = resultsList
                .OrderBy(goal => goal.IsGoalMet)
                .ThenBy(goal => goal.FundName)
                .ThenBy(goal => goal.AccountingPeriodName)
                .ToList();
        }
        else if (request.Sort == SpendingGoalSortOrderModel.IsMetDescending)
        {
            resultsList = resultsList
                .OrderByDescending(goal => goal.IsGoalMet)
                .ThenByDescending(goal => goal.FundName)
                .ThenByDescending(goal => goal.AccountingPeriodName)
                .ToList();
        }
        results = new CollectionModel<SpendingGoalModel>
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