using Domain.AccountingPeriods;
using Domain.Goals;
using Models.Goals;

namespace Rest.Goals;

/// <summary>
/// Converter class that handles converting goal domain types to REST models.
/// </summary>
public sealed class GoalConverter(IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Maps the provided assignment goal to a goal model.
    /// </summary>
    public AssignmentGoalModel ToModel(AssignmentGoal goal)
    {
        AccountingPeriod? accountingPeriod = goal.AccountingPeriodId == null ? null : accountingPeriodRepository.GetById(goal.AccountingPeriodId);
        return new AssignmentGoalModel
        {
            Id = goal.Id.Value,
            FundId = goal.Fund.Id.Value,
            FundName = goal.Fund.Name,
            AccountingPeriodId = accountingPeriod?.Id.Value,
            AccountingPeriodName = accountingPeriod?.Name,
            Type = GoalTypeConverter.ToModel(goal.AssignmentGoalType),
            GoalAmount = goal.GoalAmount,
            TotalAmountToAssign = goal.TotalAmountToAssign,
            TotalAmountAssigned = goal.TotalAmountAssigned,
            TotalAmountAssignedIncludingPending = goal.TotalAmountAssignedIncludingPending,
            RemainingAmountToAssign = Math.Max(goal.TotalAmountToAssign - goal.TotalAmountAssigned, 0),
            RemainingAmountToAssignIncludingPending = Math.Max(goal.GoalAmount - goal.TotalAmountAssignedIncludingPending, 0),
            IsGoalMet = goal.IsGoalMet,
            IsGoalMetIncludingPending = goal.IsGoalMetIncludingPending,
        };
    }

    /// <summary>
    /// Maps the provided spending goal to a goal model.
    /// </summary>
    public SpendingGoalModel ToModel(SpendingGoal goal)
    {
        AccountingPeriod? accountingPeriod = goal.AccountingPeriodId == null ? null : accountingPeriodRepository.GetById(goal.AccountingPeriodId);
        return new SpendingGoalModel
        {
            Id = goal.Id.Value,
            FundId = goal.Fund.Id.Value,
            FundName = goal.Fund.Name,
            AccountingPeriodId = accountingPeriod?.Id.Value,
            AccountingPeriodName = accountingPeriod?.Name,
            Type = GoalTypeConverter.ToModel(goal.SpendingGoalType),
            TotalAmountToSpend = goal.TotalAmountToSpend,
            TotalAmountSpent = goal.TotalAmountSpent,
            TotalAmountSpentIncludingPending = goal.TotalAmountSpentIncludingPending,
            RemainingAmountToSpend = Math.Max(goal.TotalAmountToSpend - goal.TotalAmountSpent, 0),
            RemainingAmountToSpendIncludingPending = Math.Max(goal.TotalAmountToSpend - goal.TotalAmountSpentIncludingPending, 0),
            IsGoalMet = goal.IsGoalMet,
            IsGoalMetIncludingPending = goal.IsGoalMetIncludingPending,
        };
    }
}