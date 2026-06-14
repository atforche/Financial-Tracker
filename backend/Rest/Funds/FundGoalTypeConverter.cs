using System.Diagnostics.CodeAnalysis;
using Domain.Goals;
using Models.Goals;

namespace Rest.Funds;

internal static class FundGoalTypeConverter
{
    public static bool TryToDomain(AssignmentGoalTypeModel assignmentGoalTypeModel, [NotNullWhen(true)] out AssignmentGoalType? assignmentGoalType)
    {
        assignmentGoalType = assignmentGoalTypeModel switch
        {
            AssignmentGoalTypeModel.MonthlyTarget => AssignmentGoalType.MonthlyTarget,
            AssignmentGoalTypeModel.RecurringContribution => AssignmentGoalType.RecurringContribution,
            _ => null,
        };
        return assignmentGoalType != null;
    }

    public static bool TryToDomain(SpendingGoalTypeModel spendingGoalTypeModel, [NotNullWhen(true)] out SpendingGoalType? spendingGoalType)
    {
        spendingGoalType = spendingGoalTypeModel switch
        {
            SpendingGoalTypeModel.Standard => SpendingGoalType.Standard,
            SpendingGoalTypeModel.Debt => SpendingGoalType.Debt,
            _ => null,
        };
        return spendingGoalType != null;
    }
}