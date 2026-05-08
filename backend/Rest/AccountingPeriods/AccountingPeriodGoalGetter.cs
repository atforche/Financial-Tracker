using Domain.AccountingPeriods;
using Domain.Goals;
using Models;
using Models.AccountingPeriods;
using Models.Goals;
using Rest.Goals;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving goals for an Accounting Period based on specified criteria
/// </summary>
public class AccountingPeriodGoalGetter(
    IGoalRepository goalRepository,
    GoalConverter goalConverter)
{
    /// <summary>
    /// Gets the goals within the specified Accounting Period that match the specified criteria
    /// </summary>
    public CollectionModel<GoalModel> Get(
        AccountingPeriodId accountingPeriodId,
        AccountingPeriodGoalQueryParameterModel request)
    {
        var results = goalRepository.GetAllByAccountingPeriod(accountingPeriodId).Select(goalConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(goal =>
                goal.FundName.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                goal.GoalType.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or AccountingPeriodGoalSortOrderModel.Name)
        {
            results = results.OrderBy(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.NameDescending)
        {
            results = results.OrderByDescending(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.Type)
        {
            results = results.OrderBy(goal => goal.GoalType).ThenBy(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.TypeDescending)
        {
            results = results.OrderByDescending(goal => goal.GoalType).ThenByDescending(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.GoalAmount)
        {
            results = results.OrderBy(goal => goal.GoalAmount).ThenBy(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.GoalAmountDescending)
        {
            results = results.OrderByDescending(goal => goal.GoalAmount).ThenByDescending(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.RemainingAmountToAssign)
        {
            results = results.OrderBy(goal => goal.RemainingAmountToAssign).ThenBy(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.RemainingAmountToAssignDescending)
        {
            results = results.OrderByDescending(goal => goal.RemainingAmountToAssign).ThenByDescending(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.IsAssignmentGoalMet)
        {
            results = results.OrderBy(goal => goal.IsAssignmentGoalMet).ThenBy(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.IsAssignmentGoalMetDescending)
        {
            results = results.OrderByDescending(goal => goal.IsAssignmentGoalMet).ThenByDescending(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.RemainingAmountToSpend)
        {
            results = results.OrderBy(goal => goal.RemainingAmountToSpend).ThenBy(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.RemainingAmountToSpendDescending)
        {
            results = results.OrderByDescending(goal => goal.RemainingAmountToSpend).ThenByDescending(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.IsSpendingGoalMet)
        {
            results = results.OrderBy(goal => goal.IsSpendingGoalMet).ThenBy(goal => goal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodGoalSortOrderModel.IsSpendingGoalMetDescending)
        {
            results = results.OrderByDescending(goal => goal.IsSpendingGoalMet).ThenByDescending(goal => goal.FundName).ToList();
        }
        return new CollectionModel<GoalModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }
}