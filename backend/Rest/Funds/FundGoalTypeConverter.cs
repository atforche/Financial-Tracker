using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converter class that handles converting Fund Goal types to Fund Goal type models
/// </summary>
internal sealed class FundGoalTypeConverter
{
    /// <summary>
    /// Converts the provided Fund Goal type to a Fund Goal type model
    /// </summary>
    public static FundGoalTypeModel ToModel(FundGoalType fundGoalType) => fundGoalType switch
    {
        FundGoalType.Monthly => FundGoalTypeModel.Monthly,
        FundGoalType.Rolling => FundGoalTypeModel.Rolling,
        FundGoalType.Savings => FundGoalTypeModel.Savings,
        FundGoalType.Debt => FundGoalTypeModel.Debt,
        _ => throw new InvalidOperationException($"Unrecognized Fund Goal type: {fundGoalType}")
    };

    /// <summary>
    /// Attempts to convert the provided Fund Goal type model to a Fund Goal type
    /// </summary>
    public static bool TryToDomain(FundGoalTypeModel fundGoalTypeModel, [NotNullWhen(true)] out FundGoalType? fundGoalType)
    {
        fundGoalType = fundGoalTypeModel switch
        {
            FundGoalTypeModel.Monthly => FundGoalType.Monthly,
            FundGoalTypeModel.Rolling => FundGoalType.Rolling,
            FundGoalTypeModel.Savings => FundGoalType.Savings,
            FundGoalTypeModel.Debt => FundGoalType.Debt,
            _ => null
        };
        return fundGoalType != null;
    }
}