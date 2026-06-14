using System.Diagnostics.CodeAnalysis;
using Domain.Goals;
using Models.Goals;

namespace Rest.Goals;

/// <summary>
/// Converter class that handles converting assignment and spending goal types to model types.
/// </summary>
internal sealed class GoalTypeConverter
{
    /// <summary>
    /// Converts the provided assignment goal type to a model type.
    /// </summary>
    public static AssignmentGoalTypeModel ToModel(AssignmentGoalType goalType) => goalType switch
    {
        AssignmentGoalType.MonthlyTarget => AssignmentGoalTypeModel.MonthlyTarget,
        AssignmentGoalType.RecurringContribution => AssignmentGoalTypeModel.RecurringContribution,
        _ => throw new InvalidOperationException($"Unrecognized goal type: {goalType}")
    };

    /// <summary>
    /// Converts the provided spending goal type to a model type.
    /// </summary>
    public static SpendingGoalTypeModel ToModel(SpendingGoalType goalType) => goalType switch
    {
        SpendingGoalType.Standard => SpendingGoalTypeModel.Standard,
        SpendingGoalType.Debt => SpendingGoalTypeModel.Debt,
        _ => throw new InvalidOperationException($"Unrecognized goal type: {goalType}")
    };

    /// <summary>
    /// Attempts to convert the provided assignment goal type model to a domain type.
    /// </summary>
    public static bool TryToDomain(AssignmentGoalTypeModel goalTypeModel, [NotNullWhen(true)] out AssignmentGoalType? goalType)
    {
        goalType = goalTypeModel switch
        {
            AssignmentGoalTypeModel.MonthlyTarget => AssignmentGoalType.MonthlyTarget,
            AssignmentGoalTypeModel.RecurringContribution => AssignmentGoalType.RecurringContribution,
            _ => null
        };
        return goalType != null;
    }

    /// <summary>
    /// Attempts to convert the provided spending goal type model to a domain type.
    /// </summary>
    public static bool TryToDomain(SpendingGoalTypeModel goalTypeModel, [NotNullWhen(true)] out SpendingGoalType? goalType)
    {
        goalType = goalTypeModel switch
        {
            SpendingGoalTypeModel.Standard => SpendingGoalType.Standard,
            SpendingGoalTypeModel.Debt => SpendingGoalType.Debt,
            _ => null
        };
        return goalType != null;
    }
}