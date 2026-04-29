using Domain.AccountingPeriods;
using Domain.Funds;
using Models;
using Models.AccountingPeriods;
using Models.Funds;
using Rest.Funds;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving Fund Goals for an Accounting Period based on specified criteria
/// </summary>
public class AccountingPeriodFundGoalGetter(
    IFundGoalRepository fundGoalRepository,
    FundGoalConverter fundGoalConverter)
{
    /// <summary>
    /// Gets the Fund Goals within the specified Accounting Period that match the specified criteria
    /// </summary>
    public CollectionModel<FundGoalModel> Get(
        AccountingPeriodId accountingPeriodId,
        AccountingPeriodFundGoalQueryParameterModel request)
    {
        var results = fundGoalRepository.GetAllByAccountingPeriod(accountingPeriodId).Select(fundGoalConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(fundGoal =>
                fundGoal.FundName.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fundGoal.GoalType.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or AccountingPeriodFundGoalSortOrderModel.Name)
        {
            results = results.OrderBy(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.NameDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.Type)
        {
            results = results.OrderBy(fundGoal => fundGoal.GoalType).ThenBy(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.TypeDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.GoalType).ThenByDescending(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.GoalAmount)
        {
            results = results.OrderBy(fundGoal => fundGoal.GoalAmount).ThenBy(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.GoalAmountDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.GoalAmount).ThenByDescending(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.RemainingAmountToAssign)
        {
            results = results.OrderBy(fundGoal => fundGoal.RemainingAmountToAssign).ThenBy(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.RemainingAmountToAssignDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.RemainingAmountToAssign).ThenByDescending(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.IsAssignmentGoalMet)
        {
            results = results.OrderBy(fundGoal => fundGoal.IsAssignmentGoalMet).ThenBy(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.IsAssignmentGoalMetDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.IsAssignmentGoalMet).ThenByDescending(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.RemainingAmountToSpend)
        {
            results = results.OrderBy(fundGoal => fundGoal.RemainingAmountToSpend).ThenBy(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.RemainingAmountToSpendDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.RemainingAmountToSpend).ThenByDescending(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.IsSpendingGoalMet)
        {
            results = results.OrderBy(fundGoal => fundGoal.IsSpendingGoalMet).ThenBy(fundGoal => fundGoal.FundName).ToList();
        }
        else if (request.Sort == AccountingPeriodFundGoalSortOrderModel.IsSpendingGoalMetDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.IsSpendingGoalMet).ThenByDescending(fundGoal => fundGoal.FundName).ToList();
        }
        return new CollectionModel<FundGoalModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }
}