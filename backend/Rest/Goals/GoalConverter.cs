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
        AccountingPeriod accountingPeriod = GetAccountingPeriod(goal.AccountingPeriodId);
        return new AssignmentGoalModel
        {
            Id = goal.Id.Value,
            FundId = goal.Fund.Id.Value,
            FundName = goal.Fund.Name,
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            Type = GoalTypeConverter.ToModel(goal.AssignmentGoalType),
            GoalAmount = goal.GoalAmount,
            TotalAmountToAssign = goal.TotalAmountToAssign,
            RemainingAmountToAssign = goal.RemainingAmountToAssign,
            RemainingAmountToAssignIncludingPending = goal.RemainingAmountToAssignIncludingPending,
            IsGoalMet = goal.IsGoalMet,
            IsGoalMetIncludingPending = goal.IsGoalMetIncludingPending,
        };
    }

    /// <summary>
    /// Maps the provided spending goal to a goal model.
    /// </summary>
    public SpendingGoalModel ToModel(SpendingGoal goal)
    {
        AccountingPeriod accountingPeriod = GetAccountingPeriod(goal.AccountingPeriodId);
        return new SpendingGoalModel
        {
            Id = goal.Id.Value,
            FundId = goal.Fund.Id.Value,
            FundName = goal.Fund.Name,
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            Type = GoalTypeConverter.ToModel(goal.SpendingGoalType),
            TotalAmountToSpend = goal.TotalAmountToSpend,
            RemainingAmountToSpend = goal.RemainingAmountToSpend,
            RemainingAmountToSpendIncludingPending = goal.RemainingAmountToSpendIncludingPending,
            IsGoalMet = goal.IsGoalMet,
            IsGoalMetIncludingPending = goal.IsGoalMetIncludingPending,
        };
    }

    private AccountingPeriod GetAccountingPeriod(AccountingPeriodId? accountingPeriodId) =>
        accountingPeriodRepository.GetById(accountingPeriodId ?? throw new InvalidOperationException("Goal must belong to an accounting period to be mapped."));
}