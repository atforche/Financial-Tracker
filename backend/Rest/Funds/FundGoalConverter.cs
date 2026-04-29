using System.Diagnostics.CodeAnalysis;
using Data.Funds;
using Domain.AccountingPeriods;
using Domain.Funds;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converter class that handles converting Fund Goals to Fund Goal Models
/// </summary>
public sealed class FundGoalConverter(
    IAccountingPeriodRepository accountingPeriodRepository,
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
            FundId = fundGoal.Fund.Id.Value,
            FundName = fundGoal.Fund.Name,
            AccountingPeriodId = fundGoal.AccountingPeriodId.Value,
            AccountingPeriodName = accountingPeriod.Name,
            GoalType = FundGoalTypeConverter.ToModel(fundGoal.GoalType),
            GoalAmount = fundGoal.GoalAmount,
            RemainingAmountToAssign = fundGoal.RemainingAmountToAssign,
            RemainingAmountToAssignIncludingPending = fundGoal.RemainingAmountToAssignIncludingPending,
            IsAssignmentGoalMet = fundGoal.IsAssignmentGoalMet,
            IsAssignmentGoalMetIncludingPending = fundGoal.IsAssignmentGoalMetIncludingPending,
            RemainingAmountToSpend = fundGoal.RemainingAmountToSpend,
            RemainingAmountToSpendIncludingPending = fundGoal.RemainingAmountToSpendIncludingPending,
            IsSpendingGoalMet = fundGoal.IsSpendingGoalMet,
            IsSpendingGoalMetIncludingPending = fundGoal.IsSpendingGoalMetIncludingPending,
        };
    }

    /// <summary>
    /// Attempts to map the provided ID to a Fund Goal
    /// </summary>
    public bool TryToDomain(Guid fundId, Guid accountingPeriodId, [NotNullWhen(true)] out FundGoal? fundGoal) =>
        fundGoalRepository.TryGetByFundAndAccountingPeriod(fundId, accountingPeriodId, out fundGoal);
}