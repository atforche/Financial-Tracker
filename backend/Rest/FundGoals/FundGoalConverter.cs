using Domain;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Models;
using Models.AccountingPeriods;
using Models.FundGoals;
using Models.Funds;

namespace Rest.FundGoals;

/// <summary>
/// Converts between Fund Goal API models and Domain query types.
/// </summary>
public sealed class FundGoalConverter
{
    /// <summary>
    /// Converts the provided Fund Goal query model to a Domain query.
    /// </summary>
    public FundGoalQuery ToDomain(FundGoalQueryParameterModel model) => new(
        new FundGoalFilter(
            model.Filter?.FundIds ?? [],
            model.Filter?.AccountingPeriodIds ?? [],
            model.Filter?.IncludeOnboarded),
        model.Sort == FundGoalSortModel.FundDescending ? FundGoalSort.FundDescending : FundGoalSort.Fund,
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Fund Goal to a Fund Goal model.
    /// </summary>
    public FundGoalModel ToModel(FundGoal fundGoal) => new()
    {
        Id = fundGoal.Id.Value,
        Fund = new FundModel
        {
            Id = fundGoal.Fund.Id.Value,
            Name = fundGoal.Fund.Name,
            Description = fundGoal.Fund.Description,
        },
        AccountingPeriod = fundGoal.AccountingPeriod == null ? null : new AccountingPeriodModel
        {
            Id = fundGoal.AccountingPeriod.Id.Value,
            Name = fundGoal.AccountingPeriod.Name,
            Year = fundGoal.AccountingPeriod.Year,
            Month = fundGoal.AccountingPeriod.Month,
            IsOpen = fundGoal.AccountingPeriod.IsOpen,
        },
        PlannedMonthlyContribution = fundGoal.PlannedMonthlyContribution,
        MinimumEndingBalance = fundGoal.MinimumEndingBalance,
        MaximumEndingBalance = fundGoal.MaximumEndingBalance,
    };

    /// <summary>
    /// Converts the provided Fund Goal page to a collection model.
    /// </summary>
    public CollectionModel<FundGoalModel> ToModel(QueryPage<FundGoal> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };
}
