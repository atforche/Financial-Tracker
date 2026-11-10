using Domain;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Models;
using Models.FundGoals;
using Models.Funds;
using Rest.AccountingPeriods;

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
            model.Filter?.IncludeOnboarded,
            model.Filter?.NameSearch),
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
        AccountingPeriod = fundGoal.AccountingPeriod == null ? null : AccountingPeriodConverter.ToModel(fundGoal.AccountingPeriod),
        PlannedMonthlyContribution = fundGoal.PlannedMonthlyContribution,
        MinimumEndingBalance = fundGoal.MinimumEndingBalance,
        MaximumEndingBalance = fundGoal.MaximumEndingBalance,
        AllowExpectedContributionAboveMaximum = fundGoal.AllowExpectedContributionAboveMaximum,
    };

    /// <summary>
    /// Converts the provided Fund Goal page to a collection model.
    /// </summary>
    public CollectionModel<FundGoalModel> ToModel(QueryPage<FundGoal> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Maps the provided Fund Goal Progress to a Fund Goal Progress Model.
    /// </summary>
    public FundGoalProgressModel ToModel(FundGoalProgress progress) => new()
    {
        Contribution = progress.Contribution == null ? null : new ContributionProgressModel
        {
            PlannedAmount = progress.Contribution.PlannedAmount,
            ExpectedAmount = progress.Contribution.ExpectedAmount,
            AmountReducedByMaximumEndingBalance = progress.Contribution.AmountReducedByMaximumEndingBalance,
            AssignedAmount = progress.Contribution.AssignedAmount,
            RemainingAmount = progress.Contribution.RemainingAmount,
            IsSatisfied = progress.Contribution.IsSatisfied,
        },
        EndingBalance = new FundGoalEndingBalanceProgressModel
        {
            EndingBalance = progress.EndingBalance.EndingBalance,
            MinimumBalance = progress.EndingBalance.MinimumBalance,
            MaximumBalance = progress.EndingBalance.MaximumBalance,
            AmountBelowMinimum = progress.EndingBalance.AmountBelowMinimum,
            AmountAboveMaximum = progress.EndingBalance.AmountAboveMaximum,
            Status = (FundGoalEndingBalanceStatusModel)progress.EndingBalance.Status,
        },
    };
}
