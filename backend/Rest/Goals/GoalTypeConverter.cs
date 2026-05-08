using System.Diagnostics.CodeAnalysis;
using Domain.Goals;
using Models.Goals;

namespace Rest.Goals;

/// <summary>
/// Converter class that handles converting goal types to goal type models
/// </summary>
internal sealed class GoalTypeConverter
{
    /// <summary>
    /// Converts the provided goal type to a goal type model
    /// </summary>
    public static GoalTypeModel ToModel(GoalType goalType) => goalType switch
    {
        GoalType.Monthly => GoalTypeModel.Monthly,
        GoalType.Rolling => GoalTypeModel.Rolling,
        GoalType.Savings => GoalTypeModel.Savings,
        GoalType.Debt => GoalTypeModel.Debt,
        _ => throw new InvalidOperationException($"Unrecognized goal type: {goalType}")
    };

    /// <summary>
    /// Attempts to convert the provided goal type model to a goal type
    /// </summary>
    public static bool TryToDomain(GoalTypeModel goalTypeModel, [NotNullWhen(true)] out GoalType? goalType)
    {
        goalType = goalTypeModel switch
        {
            GoalTypeModel.Monthly => GoalType.Monthly,
            GoalTypeModel.Rolling => GoalType.Rolling,
            GoalTypeModel.Savings => GoalType.Savings,
            GoalTypeModel.Debt => GoalType.Debt,
            _ => null
        };
        return goalType != null;
    }
}