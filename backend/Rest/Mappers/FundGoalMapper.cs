using System.Diagnostics.CodeAnalysis;
using Data.AccountingPeriods;
using Data.Funds;
using Domain.AccountingPeriods;
using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Goals to Fund Goal Models
/// </summary>
public sealed class FundGoalMapper(
    AccountingPeriodRepository accountingPeriodRepository,
    FundGoalRepository fundGoalRepository)
{
    /// <summary>
    /// Maps the provided Fund Goal to a Fund Goal Model
    /// </summary>
    public FundGoalModel ToModel(FundGoal fundGoal)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(fundGoal.AccountingPeriodId);
        return new FundGoalModel
        {
            Id = fundGoal.Id.Value,
            AccountingPeriodId = fundGoal.AccountingPeriodId.Value,
            AccountingPeriodName = accountingPeriod.Name,
            GoalAmount = fundGoal.GoalAmount,
            IsGoalMet = fundGoal.IsGoalMet,
        };
    }

    /// <summary>
    /// Attempts to map the provided ID to a Fund Goal
    /// </summary>
    public bool TryToDomain(Guid fundGoalId, [NotNullWhen(true)] out FundGoal? fundGoal) =>
        fundGoalRepository.TryGetById(fundGoalId, out fundGoal);
}