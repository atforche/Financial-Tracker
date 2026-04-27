using System.Globalization;
using Domain.Funds;
using Models;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Class that handles retrieving Fund Goals based on specified criteria
/// </summary>
public class FundGoalGetter(IFundGoalRepository fundGoalRepository, FundGoalConverter fundGoalConverter)
{
    /// <summary>
    /// Gets the Fund Goals for the provided Fund that match the specified criteria
    /// </summary>
    public CollectionModel<FundGoalModel> Get(FundId fundId, FundGoalQueryParameterModel request)
    {
        var results = fundGoalRepository.GetAllByFund(fundId).Select(fundGoalConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(fundGoal =>
                fundGoal.AccountingPeriodName.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fundGoal.GoalType.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fundGoal.GoalAmount.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or FundGoalSortOrderModel.AccountingPeriod)
        {
            results = results.OrderBy(fundGoal => fundGoal.AccountingPeriodName).ToList();
        }
        else if (request.Sort == FundGoalSortOrderModel.AccountingPeriodDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.AccountingPeriodName).ToList();
        }
        else if (request.Sort == FundGoalSortOrderModel.GoalAmount)
        {
            results = results.OrderBy(fundGoal => fundGoal.GoalAmount).ThenBy(fundGoal => fundGoal.AccountingPeriodName).ToList();
        }
        else if (request.Sort == FundGoalSortOrderModel.GoalAmountDescending)
        {
            results = results.OrderByDescending(fundGoal => fundGoal.GoalAmount).ThenByDescending(fundGoal => fundGoal.AccountingPeriodName).ToList();
        }
        return new CollectionModel<FundGoalModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count
        };
    }
}