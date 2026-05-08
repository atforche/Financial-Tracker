using Domain.AccountingPeriods;
using Domain.Goals;
using Models.Goals;

namespace Rest.Goals;

/// <summary>
/// Converter class that handles converting goals to goal models
/// </summary>
public sealed class GoalConverter(IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Maps the provided goal to a goal model
    /// </summary>
    public GoalModel ToModel(Goal goal)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(goal.AccountingPeriodId);
        return new GoalModel
        {
            Id = goal.Id.Value,
            FundId = goal.Fund.Id.Value,
            FundName = goal.Fund.Name,
            AccountingPeriodId = goal.AccountingPeriodId.Value,
            AccountingPeriodName = accountingPeriod.Name,
            GoalType = GoalTypeConverter.ToModel(goal.GoalType),
            GoalAmount = goal.GoalAmount,
            RemainingAmountToAssign = goal.RemainingAmountToAssign,
            RemainingAmountToAssignIncludingPending = goal.RemainingAmountToAssignIncludingPending,
            IsAssignmentGoalMet = goal.IsAssignmentGoalMet,
            IsAssignmentGoalMetIncludingPending = goal.IsAssignmentGoalMetIncludingPending,
            RemainingAmountToSpend = goal.RemainingAmountToSpend,
            RemainingAmountToSpendIncludingPending = goal.RemainingAmountToSpendIncludingPending,
            IsSpendingGoalMet = goal.IsSpendingGoalMet,
            IsSpendingGoalMetIncludingPending = goal.IsSpendingGoalMetIncludingPending,
        };
    }
}