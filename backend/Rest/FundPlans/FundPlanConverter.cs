using Domain;
using Domain.FundPlans;
using Domain.FundPlans.Queries;
using Models;
using Models.AccountingPeriods;
using Models.FundPlans;
using Models.Funds;

namespace Rest.FundPlans;

/// <summary>
/// Converts between Fund Plan API models and Domain query types.
/// </summary>
public sealed class FundPlanConverter
{
    /// <summary>
    /// Converts the provided Fund Plan query model to a Domain query.
    /// </summary>
    public FundPlanQuery ToDomain(FundPlanQueryParameterModel model) => new(
        new FundPlanFilter(
            model.Filter?.FundIds ?? [],
            model.Filter?.AccountingPeriodIds ?? [],
            model.Filter?.IncludeOnboarded),
        model.Sort == FundPlanSortModel.FundDescending ? FundPlanSort.FundDescending : FundPlanSort.Fund,
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Fund Plan to a Fund Plan model.
    /// </summary>
    public FundPlanModel ToModel(FundPlan plan) => new()
    {
        Id = plan.Id.Value,
        Fund = new FundModel
        {
            Id = plan.Fund.Id.Value,
            Name = plan.Fund.Name,
            Description = plan.Fund.Description,
        },
        AccountingPeriod = plan.AccountingPeriod == null ? null : new AccountingPeriodModel
        {
            Id = plan.AccountingPeriod.Id.Value,
            Name = plan.AccountingPeriod.Name,
            Year = plan.AccountingPeriod.Year,
            Month = plan.AccountingPeriod.Month,
            IsOpen = plan.AccountingPeriod.IsOpen,
        },
        RegularContribution = plan.RegularContribution,
        MinimumFundedBalance = plan.MinimumFundedBalance,
        MaximumFundedBalance = plan.MaximumFundedBalance,
        TargetEndingBalance = plan.TargetEndingBalance,
    };

    /// <summary>
    /// Converts the provided Fund Plan page to a collection model.
    /// </summary>
    public CollectionModel<FundPlanModel> ToModel(QueryPage<FundPlan> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };
}