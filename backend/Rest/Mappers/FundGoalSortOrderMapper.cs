using System.Diagnostics.CodeAnalysis;
using Data.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Goal Sort Orders to Fund Goal Sort Order Models
/// </summary>
internal sealed class FundGoalSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Fund Goal Sort Order Model to a Fund Goal Sort Order
    /// </summary>
    public static bool TryToData(
        FundGoalSortOrderModel fundGoalSortOrderModel,
        [NotNullWhen(true)] out FundGoalSortOrder? fundGoalSortOrder)
    {
        fundGoalSortOrder = fundGoalSortOrderModel switch
        {
            FundGoalSortOrderModel.AccountingPeriod => FundGoalSortOrder.AccountingPeriod,
            FundGoalSortOrderModel.AccountingPeriodDescending => FundGoalSortOrder.AccountingPeriodDescending,
            FundGoalSortOrderModel.GoalAmount => FundGoalSortOrder.GoalAmount,
            FundGoalSortOrderModel.GoalAmountDescending => FundGoalSortOrder.GoalAmountDescending,
            FundGoalSortOrderModel.IsGoalMet => FundGoalSortOrder.IsGoalMet,
            FundGoalSortOrderModel.IsGoalMetDescending => FundGoalSortOrder.IsGoalMetDescending,
            _ => null,
        };
        return fundGoalSortOrder != null;
    }
}